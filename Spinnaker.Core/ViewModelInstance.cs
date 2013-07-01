using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Spinnaker.Core
{
    public class ViewModelInstance : IDisposable
    {
        private INotifyPropertyChanged viewModel;
        private Guid id = Guid.NewGuid();
        private string scriptId;
        private ViewModelDefinition viewModelDefinition;
        private BrowserBridge browser;
        private Dictionary<string, List<string>> instantiatedCollectionMembers = new Dictionary<string, List<string>>();
        private Dictionary<string, ViewModelInstance> assignedViewModels = new Dictionary<string, ViewModelInstance>();

        internal ViewModelInstance(INotifyPropertyChanged viewModel, ViewModelDefinition viewModelDefinition, BrowserBridge browser)
        {
            this.viewModel = viewModel;
            this.browser = browser;
            this.viewModelDefinition = viewModelDefinition;
            scriptId = id.ToString().Replace("{", "").Replace("}", "").Replace("-", "");

            browser.ExecuteScriptFunction("createInstance", new string[] { viewModelDefinition.BindableName, scriptId });
        }

        internal void InitializeProperties()
        {
            foreach (ExposedProperty exposedProperty in viewModelDefinition.exposedProperties.Values)
            {
                if (exposedProperty.Kind == ExposedProperty.PropertyKind.ViewModel)
                {
                    ViewModelDefinition propDefinition = SpinnakerConfiguration.CurrentConfig.ViewModelManager.GetViewModelDefinition(exposedProperty.PropertyType);
                    if (propDefinition != null)
                    {
                        INotifyPropertyChanged childViewModel = exposedProperty.PropertyValue(viewModel) as INotifyPropertyChanged;
                        if (childViewModel != null)
                        {
                            ViewModelInstance childInstance = SpinnakerConfiguration.CurrentConfig.ViewModelManager.InstantiateInBrowser(childViewModel, browser);
                            browser.ExecuteScriptFunction(exposedProperty.SetterScriptFunctionName, scriptId, "@REF" + childInstance.scriptId);
                            assignedViewModels[exposedProperty.Name] = childInstance;
                        }
                    }
                }
                else
                {
                    HandleViewModelPropertyChanged(null, new PropertyChangedEventArgs(exposedProperty.Name));
                }
            }

            viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
            viewModel.PropertyChanged += HandleViewModelPropertyChanged;
        }

        public int RefCount { get; internal set; }

        public INotifyPropertyChanged ViewModel
        {
            get { return viewModel; }
        }

        internal Guid Id
        {
            get { return id; }
        }

        internal string ScriptId
        {
            get { return scriptId; }
        }

        internal BrowserBridge Browser
        {
            get { return browser; }
        }

        internal void InvokeViewModelMethod(string methodName)
        {
            ExposedMethod method = null;
            if (viewModelDefinition.exposedMethods.TryGetValue(methodName, out method))
            {
                try
                {
                    method.Invoke(viewModel);
                }
                catch (Exception ex)
                {
					SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Warn, "Exception while invoking ViewModel method " + methodName, ex);
                }
            }
        }

        internal void InvokeViewModelMethod(string methodName, string arg)
        {
            ExposedMethod method = null;
            if (viewModelDefinition.exposedMethods.TryGetValue(methodName, out method))
            {
                try
                {
                    method.Invoke(viewModel, new object[] { arg });
                }
                catch (Exception ex)
                {
					SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Warn, "Exception while invoking ViewModel method " + methodName, ex);
                }
            }
        }

        internal void HandleScriptPropertyChanged(string propertyName, string newValue)
        {
            ExposedProperty prop = null;
            if (viewModelDefinition.exposedProperties.TryGetValue(propertyName, out prop))
            {
                try
                {
                    prop.SetHostValue(viewModel, newValue);
                    browser.ExecuteScriptFunction(prop.clearValidationErrorScriptFunctionName, scriptId);
                }
                catch (TargetInvocationException tEx)
                {
                    string text = tEx.InnerException.Message;
                    if (text.Contains("\n"))
                        text = text.Substring(0, text.IndexOf("\n"));
                    browser.ExecuteScriptFunction(prop.setValidationErrorScriptFunctionName, scriptId, text);
                }
                catch (Exception ex)
                {
                    string text = ex.Message;
                    if (text.Contains("\n"))
                        text = text.Substring(0, text.IndexOf("\n"));
                    browser.ExecuteScriptFunction(prop.setValidationErrorScriptFunctionName, scriptId, text);
                }
            }
        }

        private void HandleViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            browser.InvokeOnBrowserSafeThread(() =>
            {
                ExposedProperty exposedProperty = null;
                if (viewModelDefinition.exposedProperties.TryGetValue(e.PropertyName, out exposedProperty))
                {
                    if (exposedProperty.IsEnumerableViewModels)
                    {
						ViewModelManager manager = SpinnakerConfiguration.CurrentConfig.ViewModelManager;

                        List<string> ids = new List<string>();
                        foreach (INotifyPropertyChanged item in (System.Collections.IEnumerable)exposedProperty.PropertyValue(viewModel))
                        {
                            ViewModelInstance viewModelInstance = manager.InstantiateInBrowser(item, browser);
                            ids.Add(viewModelInstance.ScriptId);
                            browser.ExecuteScriptFunction("referenceInstance", viewModelInstance.ScriptId);
                        }
                        string idsJson = Newtonsoft.Json.JsonConvert.SerializeObject(ids);
                        browser.ExecuteScriptFunction("setViewModelObservableArray", scriptId, exposedProperty.Name, exposedProperty.StringValue(viewModel), idsJson);

						List<string> previousIds = null;
						if (instantiatedCollectionMembers.TryGetValue(e.PropertyName, out previousIds))
						{
	                        foreach (string previousId in previousIds)
	                        {
								if (--manager.GetViewModelInstance(previousId).RefCount <= 0)
								{
	                                browser.ExecuteScriptFunction("releaseInstance", previousId);
	                                manager.ReleaseInBrowser(previousId);
	                            }
	                        }
						}
                        instantiatedCollectionMembers[e.PropertyName] = ids;
                    }
                    else if (exposedProperty.Kind == ExposedProperty.PropertyKind.ViewModel)
                    {
						ViewModelManager manager = SpinnakerConfiguration.CurrentConfig.ViewModelManager;
                        ViewModelInstance assignedViewModelInstance = null;
                        assignedViewModels.TryGetValue(e.PropertyName, out assignedViewModelInstance);
                        INotifyPropertyChanged childViewModel = exposedProperty.PropertyValue(viewModel) as INotifyPropertyChanged;
                        ViewModelInstance childViewModelInstance = null;
                        if (childViewModel != null)
						{
                            childViewModelInstance = manager.InstantiateInBrowser(childViewModel, browser);
							assignedViewModels[e.PropertyName] = childViewModelInstance;
                            browser.ExecuteScriptFunction("setViewModelReferenceProperty", scriptId, exposedProperty.Name, childViewModelInstance.ScriptId);
                            //browser.ExecuteScriptFunction("releaseInstance", "bleh");
                        }
                        if (assignedViewModelInstance != null && assignedViewModelInstance != childViewModelInstance)
                        {
                            if (--assignedViewModelInstance.RefCount <= 0)
                                manager.ReleaseInBrowser(assignedViewModelInstance.Id);
                        }
                    }
                    else
                        browser.ExecuteScriptFunction(exposedProperty.SetterScriptFunctionName, scriptId, exposedProperty.StringValue(viewModel));
                }
            });
        }

        public void Dispose()
        {
            if (viewModel != null)
                viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
            viewModel = null;
        }
    }
}

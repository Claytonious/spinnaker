using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spinnaker.Core
{
    public class ViewModelManager
    {
        private Dictionary<Guid, ViewModelInstance> viewModelInstancesById = new Dictionary<Guid, ViewModelInstance>();
        private Dictionary<INotifyPropertyChanged, ViewModelInstance> viewModelInstancesByViewModel = new Dictionary<INotifyPropertyChanged, ViewModelInstance>();
        private Dictionary<Type, ViewModelDefinition> viewModelDefinitions = new Dictionary<Type, ViewModelDefinition>();
		private string hostScriptObjectName;
		private string hostScriptFunctionSuffix;
        private List<Type> viewModelsDefinedInBrowser = new List<Type>();

        public ViewModelManager()
        {
			hostScriptObjectName = SpinnakerConfiguration.CurrentConfig.HostScriptObjectName;
			hostScriptFunctionSuffix = SpinnakerConfiguration.CurrentConfig.HostScriptFunctionSuffix;
		}

        internal bool IsViewModelDefinedInBrowser(Type type)
        {
            return viewModelsDefinedInBrowser.Contains(type);
        }

        internal void SetViewModelDefinedInBrowser(Type type)
        {
            viewModelsDefinedInBrowser.Add(type);
        }

        internal void RegisterViewModelDefinition(Type viewModelType, ViewModelDefinition definition)
        {
            viewModelDefinitions[viewModelType] = definition;
        }

        internal ViewModelDefinition GetViewModelDefinition(Type viewModelType)
        {
            ViewModelDefinition result = null;
            if (!viewModelDefinitions.TryGetValue(viewModelType, out result))
            {
                foreach (Type t in viewModelDefinitions.Keys)
                {
                    if (t.IsAssignableFrom(viewModelType))
                    {
                        result = viewModelDefinitions[t];
                        viewModelDefinitions[viewModelType] = result;
                        break;
                    }
                }
            }
            return result;
        }

		public IEnumerable<ViewModelInstance> CurrentInstances
		{
			get
			{
				foreach(ViewModelInstance instance in viewModelInstancesById.Values)
					yield return instance;
			}
		}

        public void InjectFramework(StringBuilder sb)
        {
            sb.Append(
                "function bindRoot(viewModelId) {\n" +
				"   try {\n" +
				"   " + hostScriptObjectName + ".HostLog" + hostScriptFunctionSuffix + "('Binding delicious roots');\n" +
                "   var viewModel = getInstance(viewModelId);\n" +
				"   " + hostScriptObjectName + ".HostLog" + hostScriptFunctionSuffix + "('View model instance ' + viewModel);\n" +
				"   ko.applyBindings(viewModel);\n" + 
				"   } catch(e) { " + hostScriptObjectName + ".HostLog" + hostScriptFunctionSuffix + "('Exception in bindRoot: ' + e.toString()); }" +
                "}\n" +
                "var isHostSettingValue = false;\n" +
                "var instanceMap = {};\n" +
                "function createInstance(typeName, id) {\n" +
				"   try {\n" +
                "   var newInstance = eval('new ' + typeName + '()');\n" +
                "   newInstance.viewModelId(id);\n" +
                "   instanceMap[id] = newInstance;\n" +
				"   }\n" +
				"   catch(e) {\n" +
				"       " + hostScriptObjectName + ".HostLog" + hostScriptFunctionSuffix + "('Exception in createInstance: ' + e);\n" +
				"   }\n" +
                "}\n" +
                "function getInstance(id) {\n" +
                "   return instanceMap[id];\n" +
                "}\n" +
                "function referenceInstance(id) {\n" +
                "   var instance = instanceMap[id];\n" +
                "   instance.refCount(instance.refCount() + 1);\n" +
                "}\n" +
                "function releaseInstance(id) {\n" +
                "   var instance = instanceMap[id];\n" +
                "   if (instance) {\n" +
                "       if (instance.refCount() <= 1){\n" +
                "           delete instanceMap[id];\n" +
                "       }\n" +
                "       else instance.refCount(instance.refCount() - 1);\n" +
                "   }\n" +
                "}\n" +
                "function setViewModelProperty(id,propName,newValue) {\n" +
				"   " + hostScriptObjectName + ".HandleScriptPropertyChanged" + hostScriptFunctionSuffix + "(id, propName, newValue);\n" +
                "}\n" +
                "function setViewModelReferenceProperty(targetId,propName,referencedId) {\n" +
                "   var referencedVm = instanceMap[referencedId];\n" +
                "   var targetVm = instanceMap[targetId];\n" +
                "   isHostSettingValue = true;\n" +
                "   try {\n" +
                "       targetVm[propName](referencedVm);\n" +
                "   } catch(e) {}\n" +
                "   isHostSettingValue = false;\n" +
                "}\n" +
                "function setViewModelObservableArray(id,propName,newValueJson,idsJson) {\n" +
                "   var newCollection = [];\n" +
                "   var arr = $.parseJSON(newValueJson);\n" +
                "   isHostSettingValue = true;\n" +
                "   var ids = $.parseJSON(idsJson);\n" +
                "   for (var i = 0; i < arr.length; i++) {\n" +
                "       var item = getInstance(ids[i]);\n" +
                "       newCollection.push(item);\n" +
                "   }\n" +
                "   getInstance(id)[propName](newCollection);\n" +
                "   isHostSettingValue = false;\n" +
                "}\n" +
                "if (typeof String.prototype.startsWith != 'function') {\n" +
                "   String.prototype.startsWith = function (str){\n" +
                "       return this.slice(0, str.length) == str;\n" +
                "   }\n" +
                "}\n"
            );
        }

		public void Reset()
		{
			foreach(Guid id in new List<Guid>(viewModelInstancesById.Keys))
				ReleaseInBrowser(id);
			viewModelDefinitions.Clear();
			viewModelInstancesById.Clear();
			viewModelInstancesByViewModel.Clear();
            viewModelsDefinedInBrowser.Clear();
        }

        public void BindViewModel(INotifyPropertyChanged viewModel, BrowserBridge browser)
        {
            ViewModelDefinition definition = GetViewModelDefinition(viewModel.GetType());
            if (definition == null)
            {
                definition = new ViewModelDefinition(viewModel.GetType(), viewModel.GetType().Name);
                StringBuilder sb = new StringBuilder();
                List<ViewModelDefinition> allDefs = new List<ViewModelDefinition>();
                definition.CollectViewModelDefinitions(allDefs);
                foreach (ViewModelDefinition child in allDefs)
                    child.DefineInBrowser(sb);
                InjectFramework(sb);
                browser.InsertScript(sb.ToString());
            }
        }

        internal ViewModelInstance InstantiateInBrowser(INotifyPropertyChanged instance, BrowserBridge browser)
        {
            ViewModelInstance result = null;
            viewModelInstancesByViewModel.TryGetValue(instance, out result);
            if (result == null)
            {
                result = new ViewModelInstance(instance, GetViewModelDefinition(instance.GetType()), browser);
				result.RefCount = 1;
                viewModelInstancesById[result.Id] = result;
                viewModelInstancesByViewModel[instance] = result;
                result.InitializeProperties();
                browser.ExecuteScriptFunction("handleViewModelCreated", instance.GetType().Name, result.Id.ToString("N"));
            }
			else
				result.RefCount++;
            return result;
        }

        internal void ReleaseInBrowser(string scriptId)
        {
            Guid id = Guid.Parse(scriptId);
            ReleaseInBrowser(id);
        }

        internal void ReleaseInBrowser(Guid id)
        {
            ViewModelInstance instance = null;
            if (viewModelInstancesById.TryGetValue(id, out instance))
            {
                viewModelInstancesById.Remove(id);
                viewModelInstancesByViewModel.Remove(instance.ViewModel);
				instance.Dispose();
			}
        }

        internal ViewModelInstance GetViewModelInstance(string scriptId)
        {
            Guid id = Guid.Parse(scriptId);
            return GetViewModelInstance(id);
        }

        internal ViewModelInstance GetViewModelInstance(Guid id)
        {
            ViewModelInstance instance = null;
            viewModelInstancesById.TryGetValue(id, out instance);
            return instance;
        }

        internal ViewModelInstance GetViewModelInstance(INotifyPropertyChanged viewModel)
        {
            ViewModelInstance instance = null;
            viewModelInstancesByViewModel.TryGetValue(viewModel, out instance);
            return instance;
        }

        public void ActivateRootViewModel(INotifyPropertyChanged viewModel, BrowserBridge browser)
        {
            ViewModelInstance rootViewModel = InstantiateInBrowser(viewModel, browser);
            browser.ExecuteScriptFunction("bindRoot", rootViewModel.ScriptId);
        }

        public void HandleScriptPropertyChanged(string id, string propertyName, string newValue)
        {
            ViewModelInstance instance = null;
            if (viewModelInstancesById.TryGetValue(Guid.Parse(id), out instance))
                instance.HandleScriptPropertyChanged(propertyName, newValue);
        }

        public void InvokeViewModelMethod(string id, string methodName)
        {
            ViewModelInstance instance = null;
            if (viewModelInstancesById.TryGetValue(Guid.Parse(id), out instance))
            {
                instance.InvokeViewModelMethod(methodName);
            }
        }

        public void InvokeViewModelMethod(string id, string methodName, string arg)
        {
            ViewModelInstance instance = null;
            if (viewModelInstancesById.TryGetValue(Guid.Parse(id), out instance))
            {
                instance.InvokeViewModelMethod(methodName, arg);
            }
        }
    }
}

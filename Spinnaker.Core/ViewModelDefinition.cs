using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spinnaker.Core
{
    internal class ViewModelDefinition
    {
        internal Dictionary<string, ExposedProperty> exposedProperties = new Dictionary<string, ExposedProperty>();
        internal Dictionary<string, ExposedMethod> exposedMethods = new Dictionary<string, ExposedMethod>();
        private string bindableName;
        private Dictionary<string,ViewModelDefinition> childViewModels = new Dictionary<string,ViewModelDefinition>();
        private Type viewModelType;

        public ViewModelDefinition(Type viewModelType, string bindableName)
        {
            SpinnakerConfiguration.CurrentConfig.ViewModelManager.RegisterViewModelDefinition(viewModelType, this);

            this.viewModelType = viewModelType;
            this.bindableName = bindableName;

            foreach (PropertyInfo propInfo in viewModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                bool include = true;
                foreach(object o in propInfo.GetCustomAttributes(true))
                {
                    if (o is Newtonsoft.Json.JsonIgnoreAttribute)
                        include = false;
                    else
                    {
                        ViewModelBinding a = o as ViewModelBinding;
                        if (a != null)
                            include = a.Binding != Binding.NotBound;
                    }
                }
                if (include)
                {
                    ExposedProperty prop = new ExposedProperty(viewModelType, propInfo);

                    if (IsTypeEnumerableViewModels(prop.PropertyType))
                    {
                        Type itemType = propInfo.PropertyType.GetGenericArguments()[0];
                        prop.CollectionType = itemType;
                        ViewModelDefinition childDefinition = SpinnakerConfiguration.CurrentConfig.ViewModelManager.GetViewModelDefinition(itemType);
                        if (childDefinition == null)
                            childDefinition = new ViewModelDefinition(itemType, itemType.Name);
                        childViewModels[propInfo.Name] = childDefinition;
                    }
                    exposedProperties[prop.Name] = prop;
                    if (typeof(INotifyPropertyChanged).IsAssignableFrom(propInfo.PropertyType))
                    {
                        ViewModelDefinition childDefinition = SpinnakerConfiguration.CurrentConfig.ViewModelManager.GetViewModelDefinition(propInfo.PropertyType);
                        if (childDefinition == null)
                            childDefinition = new ViewModelDefinition(propInfo.PropertyType, propInfo.PropertyType.Name);
                        childViewModels[propInfo.Name] = childDefinition;
                        prop.Kind = ExposedProperty.PropertyKind.ViewModel;
                    }
                }
            }

            foreach (MethodInfo methodInfo in viewModelType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!methodInfo.IsSpecialName)
                {
                    bool include = true;
                    foreach(object o in methodInfo.GetCustomAttributes(true))
                    {
                        if (o is Newtonsoft.Json.JsonIgnoreAttribute)
                            include = false;
                        else
                        {
                            ViewModelBinding a = o as ViewModelBinding;
                            if (a != null)
                                include = a.Binding != Binding.NotBound;
                        }
                    }
                    if (include)
                    {
                        if (methodInfo.GetParameters().Length == 0)
                        {
                            ExposedMethod exposedMethod = new ExposedMethod(methodInfo);
                            exposedMethods[methodInfo.Name] = exposedMethod;
                        }
                        else if (methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(string))
                        {
                            ExposedMethod exposedMethod = new ExposedMethod(methodInfo);
                            exposedMethod.IsParameterized = true;
                            exposedMethods[methodInfo.Name] = exposedMethod;
                        }
                    }
                }
            }
        }

		internal static bool IsTypeEnumerableViewModels (Type t)
		{
			bool isEnumerableOfViewModels = typeof(IEnumerable<INotifyPropertyChanged>).IsAssignableFrom(t);
			if (!isEnumerableOfViewModels)
			{
				if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					Type memberType = t.GetGenericArguments()[0];
					if (typeof(INotifyPropertyChanged).IsAssignableFrom(memberType))
						isEnumerableOfViewModels = true;
				}
			}
			return isEnumerableOfViewModels;
		}

		internal static bool IsTypeEnumerable (Type t)
		{
			bool isEnumerable = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
				t.IsAssignableFrom(typeof(System.Collections.IEnumerable));
            if (!isEnumerable)
            {
                isEnumerable = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>);
            }
			return isEnumerable;
		}

        public void CollectViewModelDefinitions(List<ViewModelDefinition> definitions)
        {
            if (!definitions.Contains(this))
            {
                definitions.Add(this);
                foreach (ViewModelDefinition child in childViewModels.Values)
                    child.CollectViewModelDefinitions(definitions);
            }
        }

        public string BindableName
        {
            get { return bindableName; }
        }

        public void DefineInBrowser(StringBuilder sb)
        {
            if (!SpinnakerConfiguration.CurrentConfig.ViewModelManager.IsViewModelDefinedInBrowser(viewModelType))
            {
                sb.Append("function " + bindableName + "() {\n");
                sb.Append("    var self = this;\n");
                sb.Append("    self.viewModelId = ko.observable(null);\n");
                sb.Append("    self.refCount = ko.observable(0);\n");
                foreach (ExposedProperty prop in exposedProperties.Values)
                {
                    string koType = "ko.observable";
                    if (prop.Kind == ExposedProperty.PropertyKind.Enumerable)
                        koType = "ko.observableArray";
                    sb.Append("    self." + prop.Name + " = " + koType + "(" + prop.ScriptLiteral + ");\n");
                    sb.Append("    self." + prop.Name + "ValidationError = ko.observable(null);\n");
                    sb.Append(prop.BuildSubscriptionScriptFunction());
                }
                foreach (ExposedMethod method in exposedMethods.Values)
                {
                    sb.Append(method.BuildInvokerScriptFunction());
                }
                sb.Append("}\n");
                foreach (ExposedProperty prop in exposedProperties.Values)
                {
                    sb.Append(prop.BuildSetterScriptFunction() + "\n");
                    sb.Append(prop.BuildValidationScriptFunctions() + "\n");
                }
                SpinnakerConfiguration.CurrentConfig.ViewModelManager.SetViewModelDefinedInBrowser(viewModelType);
            }
        }
    }

    internal class ExposedProperty
    {
        internal enum PropertyKind
        {
            IntScriptType,
            FloatScriptType,
            BoolScriptType,
            StringableType,
            Enumerable,
            ViewModel,
			Enum
        }

        internal Type PropertyType
        {
            get { return propInfo.PropertyType; }
        }

        internal Type CollectionType
        {
            get { return collectionType; }
            set
            {
                collectionType = value;
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(collectionType))
                    isEnumerableViewModels = true;
            }
        }

        internal bool IsEnumerableViewModels
        {
            get { return isEnumerableViewModels; }
        }

        private PropertyInfo propInfo;
        private PropertyKind kind;
        private string setterScriptFunctionName;
        internal string setValidationErrorScriptFunctionName;
        internal string clearValidationErrorScriptFunctionName;
        private bool isEnumerableViewModels;
        private Type collectionType;
		private static string hostScriptObjectName;
		private static string hostScriptFunctionSuffix;
		private static object[] emptyArgs = new object[0];

        internal object PropertyValue(INotifyPropertyChanged host)
        {
            return propInfo.GetValue(host, emptyArgs);
        }
           
        private readonly static List<Type> intScriptTypes = new List<Type>() 
        { 
            typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(byte)
        };

        private readonly static List<Type> floatScriptTypes = new List<Type>() 
        { 
            typeof(float), typeof(double), typeof(decimal)
        };

        internal ExposedProperty (Type hostType, PropertyInfo propInfo)
		{
			if (hostScriptObjectName == null) 
			{
				hostScriptObjectName = SpinnakerConfiguration.CurrentConfig.HostScriptObjectName;
				hostScriptFunctionSuffix = SpinnakerConfiguration.CurrentConfig.HostScriptFunctionSuffix;
			}

            this.propInfo = propInfo;
            setterScriptFunctionName = "set" + hostType.Name + "_" + propInfo.Name;
            setValidationErrorScriptFunctionName = "set" + hostType.Name + "ValidationError_" + propInfo.Name;
            clearValidationErrorScriptFunctionName = "clear" + hostType.Name + "ValidationError_" + propInfo.Name;
            if (ViewModelDefinition.IsTypeEnumerable(propInfo.PropertyType))
                kind = PropertyKind.Enumerable;
            else if (intScriptTypes.Contains(propInfo.PropertyType))
                kind = PropertyKind.IntScriptType;
            else if (floatScriptTypes.Contains(propInfo.PropertyType))
                kind = PropertyKind.FloatScriptType;
            else if (propInfo.PropertyType == typeof(bool))
                kind = PropertyKind.BoolScriptType;
			else if (propInfo.PropertyType.IsEnum)
				kind = PropertyKind.Enum;
            else if (typeof(INotifyPropertyChanged).IsAssignableFrom(propInfo.PropertyType))
                kind = PropertyKind.ViewModel;
            else
                kind = PropertyKind.StringableType;
        }

        internal int Count(INotifyPropertyChanged host)
        {
            return kind != PropertyKind.Enumerable ? 0 : ((ICollection<object>)propInfo.GetValue(host, emptyArgs)).Count;
        }

        internal PropertyKind Kind 
		{ 
			get { return kind; } 
			set { kind = value; } 
		}

        internal void SetHostValue(INotifyPropertyChanged host, string newValue)
        {
            object valToSet = null;
            try
            {
                if (propInfo.PropertyType == typeof(string))
                    valToSet = newValue;
				else if (kind == PropertyKind.Enum)
					valToSet = Enum.Parse(propInfo.PropertyType, newValue);
                else
                    valToSet = Convert.ChangeType(newValue, propInfo.PropertyType);
            }
            catch (Exception ex)
            {
				SpinnakerConfiguration.CurrentConfig.Log(SpinnakerLogLevel.Warn,"Property from view failed to convert to target ViewModel type", ex);
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }

            if (SpinnakerConfiguration.CurrentConfig.PropertyChangeHandler != null)
            {
                SpinnakerConfiguration.CurrentConfig.PropertyChangeHandler(host, () =>
                {
                    propInfo.SetValue(host, valToSet, emptyArgs);
                });
            }
            else
                propInfo.SetValue(host, valToSet, emptyArgs);
        }

        internal string Name
        {
            get { return propInfo.Name; }
        }

        internal string BuildValidationScriptFunctions()
        {
            return "function " + setValidationErrorScriptFunctionName + "(id,msg) {\n" +
                "    var instance = instanceMap[id];\n" +
                "    instance." + propInfo.Name + "ValidationError(msg);\n" +
                "}\n" +
                "function " + clearValidationErrorScriptFunctionName + "(id) {\n" +
                "    var instance = instanceMap[id];\n" +
                "    instance." + propInfo.Name + "ValidationError(null);\n" +
                "}";
        }

        internal string BuildSetterScriptFunction()
        {
            if (kind == PropertyKind.Enumerable)
            {
                return "function " + setterScriptFunctionName + "(id,newVal) {\n" +
                    "   isHostSettingValue = true;\n" +
                    "   var instance = instanceMap[id];\n" +
                    "   instance." + propInfo.Name + "($.parseJSON(newVal));\n" +
                    "   isHostSettingValue = false;\n" +
                    "}";
            }
            else
            {
                string assignment = null;

                if (kind == PropertyKind.BoolScriptType)
                    assignment = "    var valToSet = newVal==='t';\n";
                else if (kind == PropertyKind.FloatScriptType)
                    assignment = "    var valToSet = parseFloat(newVal);\n";
                else if (kind == PropertyKind.IntScriptType)
                    assignment = "    var valToSet = parseInt(newVal);\n";
                else
                    assignment = "    var valToSet = newVal;\n";

                return "function " + setterScriptFunctionName + "(id,newVal) {\n" +
                    "   isHostSettingValue = true;\n" +
                    "   var instance = instanceMap[id];\n" +
                    assignment +
                    "   if (typeof(valToSet) === 'string' && valToSet.startsWith('@REF')) {\n" +
                    "       var refId = valToSet.substr(4);\n" +
                    "       valToSet = getInstance(refId);\n" +
                    "   }\n" +
                    "   instance." + propInfo.Name + "(valToSet);\n" +
                    "   isHostSettingValue = false;\n" +
                    "}\n";
            }
        }

        internal string BuildSubscriptionScriptFunction()
        {
            return "    self." + propInfo.Name + ".subscribe(function (newValue) {\n" +
                   "        if (!isHostSettingValue) {\n" +
					"            " + hostScriptObjectName + ".HandleScriptPropertyChanged" + hostScriptFunctionSuffix + "(self.viewModelId(),'" + propInfo.Name + "',newValue);\n" +
                   "        }\n" +
                   "    });\n";
        }

        internal string SetterScriptFunctionName
        {
            get { return setterScriptFunctionName; }
        }

        internal string ScriptLiteral
        {
            get
            {
                string result = null;
                if (kind == PropertyKind.StringableType || kind == PropertyKind.ViewModel)
                    result = "null";
				else if (kind == PropertyKind.Enum)
					result = "'" + Enum.GetName(propInfo.PropertyType, Enum.GetValues(propInfo.PropertyType).GetValue(0)) + "'";
                else if (kind == PropertyKind.Enumerable)
                    result = "[]";
                else if (kind == PropertyKind.BoolScriptType)
                    result = "false";
                else if (kind == PropertyKind.FloatScriptType)
                    result = "0.0";
                else
                    result = "0";
                return result;
            }
        }

        internal string StringValue(INotifyPropertyChanged host)
        {
            string result = null;
            object val = propInfo.GetValue(host, emptyArgs);
            if (val == null)
                result = null;
            else if (propInfo.PropertyType == typeof(bool))
                result = (bool)val ? "t" : "f";
            else if (kind == PropertyKind.Enumerable)
            {
                Newtonsoft.Json.JsonSerializerSettings jsSettings = new Newtonsoft.Json.JsonSerializerSettings();
                jsSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                result = Newtonsoft.Json.JsonConvert.SerializeObject(val, Newtonsoft.Json.Formatting.None, jsSettings);
            }
            else
                result = val.ToString();
            return result;
        }
    }

    internal class ExposedMethod
    {
        private MethodInfo methodInfo;
		private static string hostScriptObjectName;
		private static string hostScriptFunctionSuffix;

        internal bool IsParameterized { get; set; }

        internal ExposedMethod (MethodInfo methodInfo)
		{
			this.methodInfo = methodInfo;
			if (hostScriptObjectName == null) 
			{
				hostScriptObjectName = SpinnakerConfiguration.CurrentConfig.HostScriptObjectName;
				hostScriptFunctionSuffix = SpinnakerConfiguration.CurrentConfig.HostScriptFunctionSuffix;
			}
        }

        internal void Invoke(INotifyPropertyChanged host)
        {
            methodInfo.Invoke(host, new object[] { });
        }

        internal void Invoke(INotifyPropertyChanged host, params object[] args)
        {
            methodInfo.Invoke(host, args);
        }

        internal string BuildInvokerScriptFunction()
        {
            string script = null;
            if (IsParameterized)
            {
                script = "    self." + methodInfo.Name + " = function(arg) {\n" +
                         "        " + hostScriptObjectName + ".InvokeViewModelIndexMethod" + hostScriptFunctionSuffix + "(self.viewModelId(), '" + methodInfo.Name + "', arg);\n" +
                         "    };\n";
            }
            else
                script = "    self." + methodInfo.Name + " = function() {\n" +
					"        " + hostScriptObjectName + ".InvokeViewModelMethod" + hostScriptFunctionSuffix + "(self.viewModelId(), '" + methodInfo.Name + "');\n" +
                         "    };\n";
            return script;
        }
    }
}

using System;

namespace Spinnaker.Core
{
    public class ViewModelBinding : Attribute
    {
        public ViewModelBinding(Binding binding)
        {
            Binding = binding;
        }

        public Binding Binding { get; set; }
    }
}


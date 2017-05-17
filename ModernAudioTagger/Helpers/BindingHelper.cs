using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Data;

namespace ModernAudioTagger.Helpers
{
    public static class BindingHelper
    {
        public static T GetValue<T>(this BindingExpression expression, object dataItem)
        {
            if (expression == null || dataItem == null)
            {
                return default(T);
            }

            string bindingPath = expression.ParentBinding.Path.Path;
            string[] properties = bindingPath.Split('.');

            object currentObject = dataItem;
            Type currentType = null;

            for (int i = 0; i < properties.Length; i++)
            {
                currentType = currentObject.GetType();
                PropertyInfo property = currentType.GetProperty(properties[i]);
                if (property == null)
                {
                    currentObject = null;
                    break;
                }
                currentObject = property.GetValue(currentObject, null);
                if (currentObject == null)
                {
                    break;
                }
            }

            return (T)currentObject;
        }
    }
}

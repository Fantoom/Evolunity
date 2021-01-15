using System;
using UnityEngine;

namespace Evolutex.Evolunity.Attributes
{
    public class TypeSelectorAttribute : PropertyAttribute
    {
        public Type baseType;

        public TypeSelectorAttribute(Type baseType)
        {
            this.baseType = baseType;
        }
    }
}
﻿using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shared.Data
{
    public abstract class Property : IProperty
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IProperty parentProperty;
        public IProperty ParentProperty
        {
            get { return parentProperty; }
        }

        private readonly List<IProperty> childProperties;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual async void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<IProperty> ChildProperties
        {
            get { return childProperties; }
        }

        protected Property()
        {
            parentProperty = null;
            childProperties = new List<IProperty>();
        }

        protected Property(IProperty inParentProperty)
        {
            parentProperty = inParentProperty;
            childProperties = new List<IProperty>();
            if (parentProperty != null && !parentProperty.ChildProperties.Contains(this))
            {
                parentProperty.ChildProperties.Add(this);
            }
        }

        public abstract bool TryGetValue<GetValueType>(out GetValueType value);
        public abstract bool TrySetValue(object value);
    }
}

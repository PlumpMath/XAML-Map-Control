﻿// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl
{
    public partial class MapPolyline : IMapElement
    {
#if NETFX_CORE
        // For WinRT, the Locations dependency property type is declared as object
        // instead of IEnumerable. See http://stackoverflow.com/q/10544084/1136211
        private static readonly Type LocationsPropertyType = typeof(object);
#else
        private static readonly Type LocationsPropertyType = typeof(IEnumerable<Location>);
#endif
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            "Locations", LocationsPropertyType, typeof(MapPolyline),
            new PropertyMetadata(null, LocationsPropertyChanged));

        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
            "IsClosed", typeof(bool), typeof(MapPolyline),
            new PropertyMetadata(false, (o, e) => ((MapPolyline)o).UpdateGeometry()));

        /// <summary>
        /// Gets or sets the locations that define the polyline points.
        /// </summary>
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates if the polyline is closed, i.e. is a polygon.
        /// </summary>
        public bool IsClosed
        {
            get { return (bool)GetValue(IsClosedProperty); }
            set { SetValue(IsClosedProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Shape.MeasureOverride in WPF and WinRT sometimes return a Size with zero
            // width or height, whereas Shape.MeasureOverride in Silverlight occasionally
            // throws an ArgumentException, as it tries to create a Size from a negative
            // width or height, apparently resulting from a transformed geometry in Path.Data.
            // In either case it seems to be sufficient to simply return a non-zero size.
            return new Size(1, 1);
        }

        void IMapElement.ParentMapChanged(MapBase oldParentMap, MapBase newParentMap)
        {
            UpdateGeometry();
        }

        private void LocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateGeometry();
        }

        private static void LocationsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var mapPolyline = (MapPolyline)obj;
            var oldCollection = e.OldValue as INotifyCollectionChanged;
            var newCollection = e.NewValue as INotifyCollectionChanged;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= mapPolyline.LocationCollectionChanged;
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += mapPolyline.LocationCollectionChanged;
            }

            mapPolyline.UpdateGeometry();
        }
    }
}

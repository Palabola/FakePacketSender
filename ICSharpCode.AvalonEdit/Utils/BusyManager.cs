﻿using System;
using System.Collections.Generic;

namespace ICSharpCode.AvalonEdit.Utils
{
    /// <summary>
    /// This class is used to prevent stack overflows by representing a 'busy' flag
    /// that prevents reentrance when another call is running.
    /// However, using a simple 'bool busy' is not thread-safe, so we use a
    /// thread-static BusyManager.
    /// </summary>
    internal static class BusyManager
    {
        public struct BusyLock : IDisposable
        {
            public static readonly BusyLock Failed = new BusyLock(null);

            private readonly List<object> objectList;

            public BusyLock(List<object> objectList)
            {
                this.objectList = objectList;
            }

            public bool Success
            {
                get { return objectList != null; }
            }

            public void Dispose()
            {
                if (objectList != null)
                {
                    objectList.RemoveAt(objectList.Count - 1);
                }
            }
        }

        [ThreadStatic]
        private static List<object> _activeObjects;

        public static BusyLock Enter(object obj)
        {
            List<object> activeObjects = _activeObjects;
            if (activeObjects == null)
                activeObjects = _activeObjects = new List<object>();
            for (int i = 0; i < activeObjects.Count; i++)
            {
                if (activeObjects[i] == obj)
                    return BusyLock.Failed;
            }
            activeObjects.Add(obj);
            return new BusyLock(activeObjects);
        }
    }
}
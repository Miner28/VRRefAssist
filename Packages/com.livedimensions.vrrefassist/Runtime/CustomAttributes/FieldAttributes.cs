using System;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
// ReSharper disable CoVariantArrayConversion

namespace VRRefAssist
{
    /// <summary>
    /// You can create your own AutosetAttribute by inheriting this class and overriding GetObjectsLogic to return the objects you want to set the field to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class AutosetAttribute : Attribute
    {
        public readonly bool dontOverride;
        public readonly bool suppressErrors;
        
        protected AutosetAttribute(bool dontOverride = false, bool suppressErrors = false)
        {
            this.dontOverride = dontOverride;
            this.suppressErrors = suppressErrors;
        }
        public abstract object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, Type type);
    }
    
    /// <summary>
    /// This will run GetComponent(type) on the object this is attached to and set the field to the result.
    /// </summary>
    public class GetComponent : AutosetAttribute
    {
        /// <param name="dontOverride">If the field value is not null, it won't be set again. You can use this to override references</param>
        /// <param name="suppressErrors">If the reference fails to be set, the console error will be suppressed.</param>
        public GetComponent(bool dontOverride = false, bool suppressErrors = false) : base(dontOverride, suppressErrors)
        {
        }

        public override object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, Type type)
        {
            return uSharpBehaviour.GetComponents(type);
        }
    }

    public class GetComponents : GetComponent { }

    /// <summary>
    /// This will run GetComponentInChildren(type) on the object this is attached to and set the field to the result.
    /// </summary>
    public class GetComponentInChildren : AutosetAttribute
    {
        /// <param name="dontOverride">If the field value is not null, it won't be set again. You can use this to override references</param>
        /// /// <param name="suppressErrors">If the reference fails to be set, the console error will be suppressed.</param>
        public GetComponentInChildren(bool dontOverride = false, bool suppressErrors = false) : base(dontOverride, suppressErrors)
        {
        }

        public override object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, Type type)
        {
            return uSharpBehaviour.GetComponentsInChildren(type, true);
        }
    }
    
    public class GetComponentsInChildren : GetComponentInChildren { }

    /// <summary>
    /// This will run GetComponentInParent(type) on the object this is attached to and set the field to the result.
    /// </summary>
    public class GetComponentInParent : AutosetAttribute
    {
        /// <param name="dontOverride">If the field value is not null, it won't be set again. You can use this to override references</param>
        /// <param name="suppressErrors">If the reference fails to be set, the console error will be suppressed.</param>
        public GetComponentInParent(bool dontOverride = false, bool suppressErrors = false) : base(dontOverride, suppressErrors)
        {
        }

        public override object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, Type type)
        {
            return uSharpBehaviour.GetComponentsInParent(type, true);
        }
    }
    
    public class GetComponentsInParent : GetComponentInParent { }

    /// <summary>
    /// This is will run transform.parent.GetComponent(type) on the object this is attached to and set the field to the result.
    /// </summary>
    public class GetComponentInDirectParent : AutosetAttribute
    {
        /// <param name="dontOverride">If the field value is not null, it won't be set again. You can use this to override references</param>
        /// <param name="suppressErrors">If the reference fails to be set, the console error will be suppressed.</param>
        public GetComponentInDirectParent(bool dontOverride = false, bool suppressErrors = false) : base(dontOverride, suppressErrors)
        {
        }

        public override object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, Type type)
        {
            return uSharpBehaviour.transform.parent == null ? Array.Empty<Component>() : uSharpBehaviour.transform.parent.GetComponents(type);
        }
    }
    
    public class GetComponentsInDirectParent : GetComponentInDirectParent { }

    /// <summary>
    /// This will run FindObjectsOfType(type) and set the field to the result, if the field is not an array it will use the first value.
    /// </summary>
    public class FindObjectOfType : AutosetAttribute
    {
        public readonly bool includeDisabled;

        /// <param name="includeDisabled">Include components in disabled GameObjects?</param>
        /// <param name="dontOverride">If the field value is not null, it won't be set again. You can use this to override references</param>
        /// <param name="suppressErrors">If the reference fails to be set, the console error will be suppressed.</param>
        public FindObjectOfType(bool includeDisabled = true, bool dontOverride = false, bool suppressErrors = false) : base(dontOverride, suppressErrors)
        {
            this.includeDisabled = includeDisabled;
        }

        public override object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, Type type)
        {
            #if UNITY_2020_1_OR_NEWER
            return UnityEngine.Object.FindObjectsOfType(type, includeDisabled);
            #else
            return includeDisabled ? FindObjectsOfTypeIncludeDisabled(type) : UnityEngine.Object.FindObjectsOfType(type);
            #endif
        }
        
        private static Component[] FindObjectsOfTypeIncludeDisabled(Type type)
        {
            if (type == null) return Array.Empty<Component>();

            GameObject[] rootGos = SceneManager.GetActiveScene().GetRootGameObjects();

            List<Component> objs = new List<Component>();

            foreach (GameObject root in rootGos)
            {
                objs.AddRange(root.GetComponentsInChildren(type, true));
            }

            return objs.ToArray();
        }
    }

    /// <summary>
    /// Exactly the same as FindObjectOfType, as it already works with array fields.
    /// </summary>
    public class FindObjectsOfType : FindObjectOfType { }

    /// <summary>
    /// This will run Find(searchName) and set the field to the result, it also works for type of GameObject.
    /// </summary>
    public class Find : AutosetAttribute
    {
        public readonly string searchName;

        /// <param name="searchName">The name of the object to find</param>
        /// <param name="dontOverride">If the field value is not null, it won't be set again. You can use this to override references</param>
        /// <param name="suppressErrors">If the reference fails to be set, the console error will be suppressed.</param>
        public Find(string searchName, bool dontOverride = false, bool suppressErrors = false) : base(dontOverride, suppressErrors)
        {
            this.searchName = searchName;
        }

        public override object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, Type type)
        {
            GameObject findGo = GameObject.Find(searchName);

            if (type == typeof(GameObject)) return new object[] {findGo};

            return findGo == null ? Array.Empty<Component>() : findGo.GetComponents(type);
        }
    }

    /// <summary>
    /// This will run transform.Find(searchName) and set the field to the result, it also works for type of GameObject.
    /// </summary>
    public class FindInChildren : AutosetAttribute
    {
        public readonly string searchName;

        /// <param name="searchName">The name of the object to find</param>
        /// <param name="dontOverride">If the field value is not null, it won't be set again. You can use this to override references</param>
        /// <param name="suppressErrors">If the reference fails to be set, the console error will be suppressed.</param>
        public FindInChildren(string searchName, bool dontOverride = false, bool suppressErrors = false) : base(dontOverride, suppressErrors)
        {
            this.searchName = searchName;
        }

        public override object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, Type type)
        {
            GameObject findInChildrenGo = uSharpBehaviour.transform.Find(searchName).gameObject;

            if (type == typeof(GameObject)) return new object[] {findInChildrenGo};

            return findInChildrenGo == null ? Array.Empty<Component>() : findInChildrenGo.GetComponents(type);
        }
    }

    /// <summary>
    /// This will run GameObject.FindGameObjectsWithTag(tag) and GetComponents(type) on each result. Also works for GameObjects and Transforms.
    /// By default, this will include disabled gameObjects, but this can be changed with 'includeDisabledGameObjects'. Disabled *components* are always included.
    /// </summary>
    public class FindObjectWithTag : AutosetAttribute
    {
		public readonly string tag;
        public bool includeDisabledGameObjects;
		public FindObjectWithTag(string tag, bool includeDisabledGameObjects = true, bool dontOverride = false, bool suppressErrors = false) : base(dontOverride, suppressErrors)
        {
            this.tag = tag;
            this.includeDisabledGameObjects = includeDisabledGameObjects;
        }

        public override object[] GetObjectsLogic(UdonSharpBehaviour uSharpBehaviour, System.Type type)
        {
            GameObject[] gameObjectsWithTag = FindGameObjectsWithTagIncludingDisabled(tag); //GameObject.FindGameObjectsWithTag does not actually include disabled objects, so we use this instead.

            //The order is undefined, so we'll sort by name to help make it consistent.
            gameObjectsWithTag = gameObjectsWithTag
                .OrderBy(go => go.transform.name)
                .ToArray();

            if (!includeDisabledGameObjects) {
                gameObjectsWithTag = gameObjectsWithTag.Where(go => go.activeInHierarchy).ToArray();
            }

			if (type == typeof(GameObject)) return gameObjectsWithTag;
            if (type == typeof(Transform)) return gameObjectsWithTag.Select(go => go.transform).ToArray();

			List<Component> components = new List<Component>();
            components.Capacity = Mathf.CeilToInt(gameObjectsWithTag.Length * 1.1f);
			
			foreach(GameObject go in gameObjectsWithTag) {
				components.AddRange(go.GetComponents(type));
			}

			return components.ToArray();
        }

        private static bool IsGameObjectInScene(GameObject gameObject)
        {
            //based on https://docs.unity3d.com/ScriptReference/Resources.FindObjectsOfTypeAll.html
            if (gameObject == null) return false;

            #if UNITY_EDITOR && !COMPILER_UDONSHARP
                bool isPersistent = UnityEditor.EditorUtility.IsPersistent(gameObject.transform.root.gameObject); 
                bool isHiddenOrUneditable = gameObject.hideFlags == HideFlags.NotEditable || gameObject.hideFlags == HideFlags.HideAndDontSave;
                bool isInScene = !isPersistent && !isHiddenOrUneditable;
                return isInScene;
            #else
                return true;
            #endif
        }

        private static GameObject[] FindGameObjectsWithTagIncludingDisabled(string tag)
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            if (allGameObjects == null) return Array.Empty<GameObject>();

            return allGameObjects
                .Where(go => go.CompareTag(tag))
                .Where(go => IsGameObjectInScene(go)) //Resources.FindObjectsOfTypeAll includes file assets and prefabs, so we need to filter them out.
                .ToArray();
        }
    }
    
    /// <summary>
    /// This will run GameObject.FindGameObjectsWithTag(tag) and GetComponents(type) on each result. Also works for GameObjects and Transforms.
    /// By default, this will include disabled gameObjects, but this can be changed with 'includeDisabledGameObjects'. Disabled *components* are always included.
    /// </summary>
    public class FindObjectsWithTag : FindObjectWithTag {
        public FindObjectsWithTag(string tag, bool includeDisabledGameObjects = true, bool dontOverride = false, bool suppressErrors = false) : base(tag, includeDisabledGameObjects, dontOverride, suppressErrors)
        {
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension
{
    public static GameObject FindGameObjectInChildWithTag(this GameObject parent, string tag)
    {
        if (parent == null)
            return null;

        Transform t = parent.transform;
        foreach (Transform tr in t)
        {
            Debug.Log(tr.gameObject + " ; " + tr.tag);
            if (tr.CompareTag(tag))
            {
                return tr.gameObject;
            }
            else
            {
                return tr.gameObject.FindGameObjectInChildWithTag(tag);
            }
        }
        return null;
    }

    public static T FindComponentInChildWithTag<T>(this GameObject parent, string tag) where T : Component
    {
        Transform t = parent.transform;
        foreach (Transform tr in t)
        {
            if (tr.tag == tag)
            {
                return tr.GetComponent<T>();
            }
        }
        return null;
    }
}

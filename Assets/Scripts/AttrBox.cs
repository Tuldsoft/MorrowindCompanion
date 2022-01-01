using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttrBox : MonoBehaviour
{
    AttrObj[] attrObjs = new AttrObj[Constants.AttrCount];

    public delegate int AttrGetter(AttrName attrName, bool ignoreValue = false);
    AttrGetter attrGetter;

    // run once, thereafter refresh the box
    public void PopulateAttrBox(AttrGetter getter)
    {
        attrGetter = getter;
        attrObjs[0] = gameObject.GetComponentInChildren<AttrObj>();
        GameObject attrTemplate = attrObjs[0].gameObject;

        // by index, attrObjs[] zero-based, AttrName 1-based
        for (int i = 1; i < Constants.AttrCount; i++)
        {
            GameObject attrGameObj = Instantiate(attrTemplate, attrTemplate.transform.parent);
            attrObjs[i] = attrGameObj.GetComponent<AttrObj>();

            // reposition
            Vector2 position = attrGameObj.transform.localPosition;
            position = new Vector2(position.x, position.y - (12 * i));
            attrGameObj.transform.localPosition = position;
        }
    }

    public void RefreshAttrBox()
    {
        // by index, attrObjs[] zero-based, AttrName 1-based
        for (int i = 0; i < Constants.AttrCount; i++)
        {
            AttrName attr = (AttrName)(i + 1);
            attrObjs[i].SetAttr(attr, attrGetter(attr));
        }
    }


}

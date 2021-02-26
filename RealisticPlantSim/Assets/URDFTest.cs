using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.Urdf.Editor;
using RosSharp;

public class URDFTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Q))
        {
            ImportSettings settings = new ImportSettings();
            settings.choosenAxis = ImportSettings.axisType.yAxis;
            settings.convexMethod = ImportSettings.convexDecomposer.vHACD;
            //var temp = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Resources/.urdf", typeof(object));
            var temp = Resources.Load(@"URDF/niryo_one.urdf");
            Debug.Log(temp);
            Debug.Log("Text: "+ temp.name);
            //StartCoroutine(UrdfRobotExtensions.Create(ta.name, settings));
        }
    }
}

//using Meta.XR.BuildingBlocks.AIBlocks;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class test : MonoBehaviour
//{
//    [SerializeField]
//    private ObjectDetectionAgent agent;

//    private void OnEnable()
//    {
//        agent.OnBoxesUpdated += HandleBoxes;
//    }

//    private void OnDisable()
//    {
//        agent.OnBoxesUpdated -= HandleBoxes;
//    }

//    private void HandleBoxes(List<DetectionBox> boxes)
//    {
//        foreach (var box in boxes)
//        {
//            Debug.Log(box.Label);
//        }
//    }
//}
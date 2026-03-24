using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{

    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plantVisualPrefab;

    private List<GameObject> plateVisualGameObjectList;

    private void Awake()
    {
        plateVisualGameObjectList = new List<GameObject>();
    }
    private void Start()
    {
        platesCounter.OnPlateSpawn += PlatesCounter_OnplateSpawn;
        platesCounter.OnPlateRemove += PlatesCounter_OnplateRemove;
    }

    private void PlatesCounter_OnplateRemove(object sender, System.EventArgs e)
    {
        GameObject plateVisualGameObject = plateVisualGameObjectList[plateVisualGameObjectList.Count - 1];
        plateVisualGameObjectList.Remove(plateVisualGameObject);

        Destroy(plateVisualGameObject);
    }

    private void PlatesCounter_OnplateSpawn(object sender, System.EventArgs e)
    {
       Transform plateVisualTransform=Instantiate(plantVisualPrefab,counterTopPoint);

        float plateOffsetY = 0.1f;

        plateVisualTransform.localPosition=new Vector3(0,plateOffsetY*plateVisualGameObjectList.Count,0);
        plateVisualGameObjectList.Add(plateVisualTransform.gameObject);

    }
}

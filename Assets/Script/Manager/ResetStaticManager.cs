using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetStaticManager : MonoBehaviour
{
    private void Awake()
    {
        BaseCounter.ResetStaticData();
        CuttingCounter.ResetStaticData();
        TrashCounter.ResetStaticData();
    }
}

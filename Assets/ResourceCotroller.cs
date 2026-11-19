using System;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceCotroller : MonoBehaviour
{
  
    public int Scrap = 50;

    int ASPDCost = 20;
    int DMGCost = 20;
    int RangeCost = 20;
    int GateRepair = 15;
    int GateUpgrade = 75;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
    public void AddScrap()
    {
        Scrap = Scrap + 2;
        Debug.Log("Scrap: " + Scrap);
    }
    public void BuyTower()
    {
        if (Scrap >= 50)
        {
            Scrap = Scrap - 50;
            Debug.Log("Scrap: " + Scrap);
        }
        else
        {
            Debug.Log("Not Enough Scrap");
        }

    }
    public void UpgradeASPD()
    {
        if (Scrap >= ASPDCost)
        {
            Scrap = Scrap - ASPDCost;
            ASPDCost = (int)Math.Ceiling(ASPDCost * 1.15);
            Debug.Log("Attack Speed Upgraded");
        }
        else
        {
            Debug.Log("Not Enough Scrap");
        }
    }
    public void UpgradeDMG()
    {
        if (Scrap >= DMGCost)
        {
            Scrap = Scrap - DMGCost;
            DMGCost = (int)Math.Ceiling(DMGCost * 1.15);
            Debug.Log("Damage Upgraded");
        }
        else
        {
            Debug.Log("Not Enough Scrap");
        }
    }
    public void UpgradeRange()
    {
        if (Scrap >= RangeCost)
        {
            Scrap = Scrap - RangeCost;
            RangeCost = (int)Math.Ceiling(RangeCost * 1.15);
            Debug.Log("Range Upgraded");
        }
        else
        {
            Debug.Log("Not Enough Scrap");
        }
    }
    public void RepairGate()
    {

    }
    public void UpgradeGate()
    {
        if (Scrap >= GateUpgrade)
        {
            Scrap = Scrap - GateUpgrade;
            GateUpgrade = GateUpgrade + 5;
            Debug.Log("Gate Upgraded");
        }
        else
        {
            Debug.Log("Not Enough Scrap");
        }
    }
    

}

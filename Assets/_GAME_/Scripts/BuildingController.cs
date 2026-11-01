using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Net;

public class BuildingController : MonoBehaviour
{

    private BuildingSpace[] spaces;
    [SerializeField] public GameObject highlight;    

    private void Awake()
    {

        highlight.SetActive(false);
    }

    private void Start()
    {
        
        TowerController[] towers = FindObjectsByType<TowerController>();
        spaces = new BuildingSpace[towers.Length];

        for(int i = 0; i < towers.Length; i++)
        {
            if (towers[i] == null || towers[i].gameObject == null) continue;

            var buildingSpace = new BuildingSpace();
            buildingSpace.bounds = towers[i].GetComponent<BoxCollider2D>();
            buildingSpace.tower = towers[i].towerArt;
            buildingSpace.tower.SetActive(false);
            spaces[i] = buildingSpace;
        }
    }

    void Update()
    {
        if (highlight == null || GameManager.Instance.isNight) return;
        
        highlight.SetActive(false);
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        bool clicked = Mouse.current.leftButton.wasPressedThisFrame;

        var ray = Camera.main.ScreenPointToRay(mousePosition);

        foreach(BuildingSpace space in spaces)
        {
            if(space == null || space.bounds == null || space.tower == null) continue;

            if(!space.bounds.isActiveAndEnabled) continue;

            Plane plane = new Plane(Vector3.forward, space.bounds.transform.position);            
            if(!plane.Raycast(ray, out float distance) || !space.bounds.OverlapPoint(ray.GetPoint(distance))) continue;

            highlight.SetActive(true);
            highlight.transform.position = space.bounds.gameObject.transform.position;
            highlight.transform.localScale = space.bounds.gameObject.transform.localScale;


            if (clicked && space.buildingCost <= GameManager.Instance.scrap)
            {
                space.tower.SetActive(true);
                highlight.SetActive(false);
                GameManager.Instance.SpendScrap(space.buildingCost);
            }
            
        }
    }
}

[System.Serializable]
public class BuildingSpace
{
    public BoxCollider2D bounds;
    public GameObject tower;
    public int buildingCost = 10;
}

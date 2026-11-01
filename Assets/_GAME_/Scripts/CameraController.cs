using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float DragSpeed = 0.01f;
    [SerializeField] private float zoomMultiplier = 0.002f, minZoom = 1f, maxZoom = 10f, smoothTime = .1f;

    [SerializeField] private BoxCollider2D boundsCollider;
    
    private float zoomDistance;
    private float velocity;

    void Awake()
    {
        zoomDistance = Camera.main.orthographicSize;
    }
    private void LateUpdate()
    {
        float maximum = Mathf.Min(maxZoom, Mathf.Min(boundsCollider.bounds.extents.y, boundsCollider.bounds.extents.x / Camera.main.aspect));
        if (maximum <= 0f) return;


        var minimum = Mathf.Min(Mathf.Max(0.001f, minZoom), maximum);
        var position = transform.position;

        zoomDistance -= Mouse.current.scroll.y.ReadValue() * zoomMultiplier;

        if (Mouse.current.rightButton.isPressed)
        {
            var movement = Mouse.current.delta.ReadValue();
            position.x -= movement.x * DragSpeed * zoomDistance;
            position.y -= movement.y * DragSpeed * zoomDistance;
        }

        if (Keyboard.current.wKey.isPressed) position.y += DragSpeed * zoomDistance * zoomMultiplier;
        if (Keyboard.current.sKey.isPressed) position.y -= DragSpeed * zoomDistance * zoomMultiplier;
        if (Keyboard.current.aKey.isPressed) position.x -= DragSpeed * zoomDistance * zoomMultiplier;
        if (Keyboard.current.dKey.isPressed) position.x += DragSpeed * zoomDistance * zoomMultiplier;


        zoomDistance = Mathf.Clamp(zoomDistance, minimum, maximum);

        var size = Mathf.SmoothDamp(Camera.main.orthographicSize, zoomDistance, ref velocity, smoothTime);
        Camera.main.orthographicSize = Mathf.Clamp(size, minimum, maximum);

        var halfHeight = Camera.main.orthographicSize;
        var halfWidth = halfHeight * Camera.main.aspect;

        position.x = Mathf.Clamp(position.x, boundsCollider.bounds.min.x + halfWidth, boundsCollider.bounds.max.x - halfWidth);
        position.y = Mathf.Clamp(position.y, boundsCollider.bounds.min.y + halfHeight, boundsCollider.bounds.max.y - halfHeight);

        transform.position = position;

    }

}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct StarConnection
{
    public StarNode from;
    public StarNode to;
}

public class ConstellationPuzzle : MonoBehaviour
{
    public LineRenderer linePrefab;
    [Header("Snapping")]
    [SerializeField] float snapRadius = 0.5f;
    [SerializeField] LayerMask starLayer;

    [Header("Guide Lines")]
    public LineRenderer guideLinePrefab;
    private List<LineRenderer> guideLines = new();

    private StarNode currentStar;
    private LineRenderer currentLine;

    private List<(StarNode, StarNode)> playerConnections = new();
    private List<LineRenderer> spawnedLines = new();
    public List<StarConnection> requiredConnections = new();

    public event Action OnSolved;

    private bool isActive = false;

    void Update()
    {
        if (!isActive) return;

        HandleInput();
    }

    // ================= INPUT =================

    void HandleInput()
    {
        var mouse = Mouse.current;

        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            StarNode star = GetStarUnderMouse();
            if (star != null)
                StartConnection(star);
        }

        if (mouse.leftButton.isPressed && currentLine != null)
        {
            UpdateLinePreview();
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            StarNode target = GetSnappedStarFromMouse();

            if (target != null && currentStar != null)
                TryConnect(currentStar, target);

            EndConnection();
        }
    }

    StarNode GetSnappedStarFromMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        Plane plane = new Plane(-Camera.main.transform.forward, currentStar.transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            return GetSnappedStar(worldPoint);
        }

        return null;
    }

    StarNode GetStarUnderMouse()
    {
        if (Mouse.current == null) return null;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponent<StarNode>();
        }

        return null;
    }

    // ================= CONNECTION =================

    void StartConnection(StarNode star)
    {
        currentStar = star;

        currentLine = Instantiate(linePrefab);
        currentLine.positionCount = 2;
        currentLine.SetPosition(0, star.transform.position);
    }

    void UpdateLinePreview()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        Plane plane = new Plane(-Camera.main.transform.forward, currentStar.transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);

            // 🔥 SNAP CHECK
            StarNode snapped = GetSnappedStar(worldPoint);

            if (snapped != null && snapped != currentStar)
            {
                currentLine.SetPosition(1, snapped.transform.position);
            }
            else
            {
                currentLine.SetPosition(1, worldPoint);
            }
        }
    }

    void TryConnect(StarNode from, StarNode to)
    {
        if (from == to) return;

        // prevent duplicate
        if (playerConnections.Contains((from, to)) || playerConnections.Contains((to, from)))
            return;

        if (from.IsConnectedTo(to))
        {
            currentLine.SetPosition(1, to.transform.position);

            playerConnections.Add((from, to));
            spawnedLines.Add(currentLine);

            currentLine = null;

            CheckCompletion();
        }
        else
        {
            Destroy(currentLine.gameObject);
        }
    }

    void EndConnection()
    {
        currentStar = null;

        if (currentLine != null)
            Destroy(currentLine.gameObject);
    }

    // ================= COMPLETION =================

    void CheckCompletion()
    {
        int correctConnections = 0;

        foreach (var required in requiredConnections)
        {
            if (HasConnection(required.from, required.to))
            {
                correctConnections++;
            }
        }

        if (correctConnections == requiredConnections.Count)
        {
            Solve();
        }
    }

    bool HasConnection(StarNode a, StarNode b)
    {
        return playerConnections.Contains((a, b)) ||
               playerConnections.Contains((b, a));
    }

    void Solve()
    {
        isActive = false;

        OnSolved?.Invoke();
    }

    // ================= CONTROL =================

    public void Activate()
    {
        isActive = true;
        ResetPuzzle();
        CreateGuideLines();
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void ResetPuzzle()
    {
        foreach (var line in spawnedLines)
            Destroy(line.gameObject);

        spawnedLines.Clear();
        playerConnections.Clear();

        // remove guide lines too
        foreach (var g in guideLines)
            Destroy(g.gameObject);

        guideLines.Clear();
    }

    // ================= Guide =================

    void CreateGuideLines()
    {
        foreach (var connection in requiredConnections)
        {
            LineRenderer line = Instantiate(guideLinePrefab, transform);

            line.positionCount = 2;
            line.SetPosition(0, connection.from.transform.position);
            line.SetPosition(1, connection.to.transform.position);

            guideLines.Add(line);
        }
    }

    StarNode GetSnappedStar(Vector3 worldPoint)
    {
        Collider[] hits = Physics.OverlapSphere(worldPoint, snapRadius, starLayer);

        float closestDist = float.MaxValue;
        StarNode closest = null;

        foreach (var hit in hits)
        {
            StarNode star = hit.GetComponent<StarNode>();
            if (star == null) continue;

            float dist = Vector3.Distance(worldPoint, star.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = star;
            }
        }

        return closest;
    }
}
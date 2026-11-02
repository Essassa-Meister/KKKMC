using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class PlayerTrapPlacer : MonoBehaviour
{
    [Header("設置設定")]
    public GameObject trapPrefab;
    public Transform placementOrigin;
    public float maxPlacementDistance = 5f;
    public LayerMask placementLayerMask = ~0;

    [Range(0f, 90f)]
    public float maxSlopeAngle = 30f;

    [Header("設置ムービー/アニメーション")]
    public Animator playerAnimator;
    public string placeTriggerName = "PlaceTrap";
    public float placementDuration = 1.0f;

    [Header("入力")]
    public KeyCode placeKey = KeyCode.E;

    [Header("置ける個数")]
    [SerializeField] private int maxTrapCount = 3;
    public int currentTrapCount = 0;

    private Trap lastPlacedTrap = null;

    private void Update()
    {
        if (placementOrigin == null)
        {
            return;
        }

        if (Input.GetKeyDown(placeKey))
        {
            TryPlaceTrap();
        }

        // 実際のRaycastと同じく、ワールド座標の真下へ表示する。
        Debug.DrawRay(
            placementOrigin.position,
            Vector3.down * maxPlacementDistance,
            Color.red
        );
    }

    public void TryPlaceTrap()
    {
        if (trapPrefab == null)
        {
            Debug.LogWarning("Trap Prefabが設定されていません。", this);
            return;
        }

        if (placementOrigin == null)
        {
            Debug.LogError("Placement Originが設定されていません。", this);
            return;
        }

        if (currentTrapCount >= maxTrapCount)
        {
            Debug.Log("トラップの設置上限に達しています。", this);
            return;
        }

        Ray ray = new Ray(placementOrigin.position, Vector3.down);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxPlacementDistance,
            placementLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Hit : " + hit.collider.name, hit.collider);

            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope > maxSlopeAngle)
            {
                Debug.LogWarning($"傾斜が大きすぎます: {slope:F1}度", this);
                return;
            }

            PlaceRoutine(hit.point, hit.normal).Forget();
            currentTrapCount++;
        }
        else
        {
            Debug.LogWarning(
                $"Raycast Miss: Origin={placementOrigin.position}, Distance={maxPlacementDistance}",
                this
            );
        }
    }

    private async UniTask PlaceRoutine(Vector3 position, Vector3 normal)
    {
        GameObject go = Instantiate(
            trapPrefab,
            position,
            Quaternion.FromToRotation(Vector3.up, normal)
        );

        Trap trap = go.GetComponent<Trap>();
        if (trap != null)
        {
            trap.startEnabled = false;
            trap.Initialize();
            lastPlacedTrap = trap;
        }

        if (playerAnimator != null && !string.IsNullOrEmpty(placeTriggerName))
        {
            playerAnimator.SetTrigger(placeTriggerName);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(placementDuration));

        if (lastPlacedTrap != null && !lastPlacedTrap.IsActive)
        {
            lastPlacedTrap.EnableTrap();
            lastPlacedTrap = null;
        }
    }

    // Animation Event用
    public void OnPlaceAnimationEvent()
    {
        if (lastPlacedTrap != null)
        {
            lastPlacedTrap.EnableTrap();
            lastPlacedTrap = null;
        }
    }
}

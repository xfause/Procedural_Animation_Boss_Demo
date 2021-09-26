using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLegPlacement : MonoBehaviour {

    public bool legGrounded = false;
    // 腿的实际落点
    public Vector3 stepPoint;
    public Vector3 stepNormal;

    // 理想情况下腿的落点在休息时永远沿着腿的方向向前
    public Vector3 optimalRestingPosition = new Vector3(0f, 0f, 2f);
    // 腿在静止状态下的落点
    public Vector3 restingPosition {
        get {
            return transform.TransformPoint(optimalRestingPosition);
        }
    }
    public Vector3 worldVelocity = Vector3.zero;

    // 世界坐标下的落点
    public Vector3 desiredPostion {
        get {
            return restingPosition + worldVelocity + (Random.insideUnitSphere * placementRandomization);
        }
    }

    public Vector3 worldTarget = Vector3.zero;
    public Transform ikTarget;
    public Transform ikPoleTarget;

    // 模拟随机落点
    public float placementRandomization = 0.5f;
    public bool autoStep = true;

    // 射线停止的图层
    public LayerMask solidLayer;
    public float stepRadius = 0.25f;
    // 高度动作的曲线和倍率
    public AnimationCurve stepHeightCurve;
    public float stepHeightMultiplier = 0.25f;
    // 两步之间的间隔
    public float stepCooldown = 1f;
    // 每一步抬起落下之间的间隔
    public float stepDuration = 0.5f;
    public float stepOffset;
    public float lastStep = 0;

    public float percent {
        get {
            return Mathf.Clamp01((Time.time - lastStep) / stepDuration);
        }
    }

    // Start is called before the first frame update
    void Start() {
        worldVelocity = Vector3.zero;
        lastStep = Time.time + stepCooldown * stepOffset;
        ikTarget.position = restingPosition;
        Step();
    }

    // Update is called once per frame
    void Update() {
        UpdateIkTarget();
        if (Time.time > lastStep + stepCooldown && autoStep){
            Step();
        }
    }

    public void UpdateIkTarget(){
        stepPoint = ChangePosition(worldTarget + worldVelocity);
        ikTarget.position = Vector3.Lerp(ikTarget.position, stepPoint, percent) + stepNormal * stepHeightCurve.Evaluate(percent) * stepHeightMultiplier;
    }

    public void Step(){
        stepPoint = worldTarget = ChangePosition(desiredPostion);
        lastStep = Time.time;
    }

    public Vector3 ChangePosition(Vector3 position){
        Vector3 direction = position - ikPoleTarget.position;
        RaycastHit hit;
        if (Physics.SphereCast(ikPoleTarget.position, stepRadius, direction, out hit, direction.magnitude * 2f, solidLayer)){
            Debug.DrawLine(ikPoleTarget.position, hit.point, Color.green);
            position = hit.point;
            stepNormal = hit.normal;
            legGrounded = true;
        } else {
            Debug.DrawLine(ikPoleTarget.position, restingPosition, Color.red);
            position = restingPosition;
            stepNormal = Vector3.zero;
            legGrounded = false;
        }
        return position;
    }

    public void MoveVelocity(Vector3 newVelocity){
        worldVelocity = Vector3.Lerp(worldVelocity, newVelocity, 1f - percent);
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(restingPosition, worldTarget);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(worldTarget, stepPoint);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(restingPosition, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(worldTarget, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(stepPoint, 0.1f);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(ikPoleTarget.position, stepRadius);
    }

}

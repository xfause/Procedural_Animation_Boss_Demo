using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public float moveFactor = 5f;
    public Vector3 inputVelocity;
    public Vector3 worldVelocity;
    public float walkSpeed = 2f;
    public float sprintSpeed = 5f;
    public float rotateInputFactor = 10f;
    public float rotationSpeed = 10f;
    public float averageRotationRadius = 3f;
    private float mSpeed = 0;
    private float rSpeed = 0;


    public ProceduralLegPlacement[] legs;
    public int index;
    // 动态步态 防止每步之间间隔时间过长
    public bool dynamicGait = true;
    public float timeBetweenSteps = 0.25f;
    // 调节步频持续时间
    public float stepDurationRatio = 2f;
    // 通过设定最远目标点的距离调节步频快慢
    [Tooltip("while dynamicGait is true, used to calculate timeBetweenSteps")] public float maxTargetDistance = 1f;
    public float lastStep = 0;

    [Header("Alignment")]
    // 是否计算角度 用来攀爬墙面
    public bool useAlignment = true;
    public int[] nextLegTri;
    public AnimationCurve sensitivityCurve;
    // 蜘蛛身体中心到地面距离
    public float desiredSurfaceDist = -1f;
    public float dist;
    // 判定蜘蛛是否落地 区分运动方式
    public bool grounded = false;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            averageRotationRadius += legs[i].restingPosition.z;
        }
        averageRotationRadius /= legs.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (useAlignment)
        {
            CalculateOrientation();
        }

        // ControllSpiderMove();

        //动态步态
        if (dynamicGait)
        {
            if (grounded)
            {
                timeBetweenSteps = maxTargetDistance / Mathf.Max(worldVelocity.magnitude, Mathf.Abs(2 * Mathf.PI * rSpeed * Mathf.Deg2Rad * averageRotationRadius));
            }
            else
            {
                timeBetweenSteps = 0.25f;
            }
        }

        if (Time.time > lastStep + (timeBetweenSteps / legs.Length) && legs != null)
        {
            index = (index + 1) % legs.Length;
            if (legs[index] == null)
            {
                return;
            }
            for (int i = 0; i < legs.Length; i++)
            {
                legs[i].MoveVelocity(CalculateLegVelocity(i));
            }

            legs[index].stepDuration = Mathf.Min(1f, (timeBetweenSteps / legs.Length) * stepDurationRatio);
            legs[index].worldVelocity = CalculateLegVelocity(index);
            legs[index].Step();
            lastStep = Time.time;
        }
    }

    public void MoveToPos(Vector3 position, GameObject Player)
    {
        Transform Head = transform.Find("Head");

        Vector3 localInput = Vector3.ClampMagnitude(transform.TransformDirection(position), 1f);
        inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * moveFactor);
        worldVelocity = inputVelocity * walkSpeed;

        Vector3 toPlayerDirection = new Vector3(Player.transform.position.x - transform.position.x, transform.position.y, Player.transform.position.z - transform.position.z);
        Vector3 toForWard = new Vector3(Head.position.x - transform.position.x, transform.position.y, Head.position.z - transform.position.z);

        // Debug.DrawLine(transform.position, transform.position + toForWard, Color.green);

        float AngleBetween = Vector3.Angle(toPlayerDirection, toForWard);
        if (Mathf.Abs(AngleBetween) > 0f)
        {
            float isLeft = Vector3.Cross(toForWard, toPlayerDirection).y > 0 ? 1 : -1;
            rSpeed = isLeft * rotationSpeed;
            transform.Rotate(0f, rSpeed * Time.deltaTime, 0f);
        }
        transform.position += (worldVelocity * Time.deltaTime);
    }

    public void ChargeToPos(Vector3 position, GameObject Player, float chargeWalkSpeed, float chargeRotateSpeed, float chargeDuration)
    {
        Vector3 localInput = Vector3.ClampMagnitude(transform.TransformDirection(position), 1f);
        inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * moveFactor);
        worldVelocity = inputVelocity * chargeWalkSpeed;

        Transform Head = transform.Find("Head");
        Vector3 toChargeDirection = new Vector3(Player.transform.position.x - transform.position.x, transform.position.y, Player.transform.position.z - transform.position.z);
        Vector3 toForWard = new Vector3(Head.position.x - transform.position.x, transform.position.y, Head.position.z - transform.position.z);

        float AngleBetween = Vector3.Angle(toChargeDirection, toForWard);

        // Debug.DrawLine(transform.position, transform.position + toChargeDirection, Color.red, 100f);
        // Debug.DrawLine(transform.position, transform.position + toForWard, Color.green, 100f);

        if (Mathf.Abs(AngleBetween) > 0f)
        {
            float isLeft = Vector3.Cross(toForWard, toChargeDirection).y > 0 ? 1 : -1;
            rSpeed = Mathf.MoveTowards(rSpeed, isLeft * chargeRotateSpeed, rotateInputFactor * Time.deltaTime);
            transform.Rotate(0f, rSpeed * Time.deltaTime, 0f);
        }
        transform.position += (worldVelocity * Time.deltaTime);
    }

    private void ControllSpiderMove()
    {
        mSpeed = (Input.GetButton("Fire3") ? sprintSpeed : walkSpeed);
        Vector3 localInput = Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"))), 1f);
        inputVelocity = Vector3.MoveTowards(inputVelocity, localInput, Time.deltaTime * moveFactor);
        worldVelocity = inputVelocity * mSpeed;

        rSpeed = Mathf.MoveTowards(rSpeed, Input.GetAxis("Turn") * rotationSpeed, rotateInputFactor * Time.deltaTime);
        transform.Rotate(0f, rSpeed * Time.deltaTime, 0f);

        transform.position += (worldVelocity * Time.deltaTime);
    }


    public Vector3 CalculateLegVelocity(int legIndex)
    {
        Vector3 legPoint = (legs[legIndex].restingPosition);
        Vector3 legDirection = legPoint - transform.position;
        // 相对旋转的位置
        // /2因为是双向 第一部分是旋转后的腿方向的向量 第二部分是加上原点位置后相对原先落点
        Vector3 rotationalPoint = ((Quaternion.AngleAxis((rSpeed * timeBetweenSteps) / 2f, transform.up) * legDirection) + transform.position) - legPoint;
        DrawArc(transform.position, legDirection, rSpeed / 2f, 10f, Color.black, 1f);
        return rotationalPoint + (worldVelocity * timeBetweenSteps) / 2f;
    }

    // Y轴高度相关计算
    private void CalculateOrientation()
    {
        Vector3 up = Vector3.zero;
        float avgSurfaceDist = 0;
        grounded = false;

        Vector3 point, a, b, c;

        // 通过每条腿相对于身体的距离向量
        // 结果取叉积 得到腿不在同一平面上时的up向量
        for (int i = 0; i < legs.Length; i++)
        {
            point = legs[i].stepPoint;
            // 和身体位置的高度差
            avgSurfaceDist += transform.InverseTransformPoint(point).y;
            a = (transform.position - point).normalized;
            b = ((legs[nextLegTri[i]].stepPoint) - point).normalized;
            c = Vector3.Cross(a, b);
            // 根据该腿是否落地区分情况
            // 如果未落地up方向加上身体向前运动方向
            // 如果已落地up方向加上该腿地面法线的方向
            up += c * sensitivityCurve.Evaluate(c.magnitude) + (legs[i].stepNormal == Vector3.zero ? transform.forward : legs[i].stepNormal);
            grounded |= legs[i].legGrounded;

            Debug.DrawRay(point, a, Color.red, 0);

            Debug.DrawRay(point, b, Color.green, 0);

            Debug.DrawRay(point, c, Color.blue, 0);
        }

        up /= legs.Length;
        avgSurfaceDist /= legs.Length;
        dist = avgSurfaceDist;
        Debug.DrawRay(transform.position, up, Color.red, 0);

        // 通过ProjectOnPlane确定蜘蛛背面朝上的向量
        // 以此保持前进方向的正确
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, up), up), 22.5f * Time.deltaTime);
        if (grounded)
        {
            transform.Translate(0, -(-avgSurfaceDist + desiredSurfaceDist) * 0.5f, 0, Space.Self);
        }
        else
        {
            // 重力 自由下落
            transform.Translate(0, -20 * Time.deltaTime, 0, Space.World);
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, averageRotationRadius);
    }

    public void DrawArc(Vector3 point, Vector3 dir, float angle, float stepSize, Color color, float duration)
    {
        if (angle < 0)
        {
            for (float i = 0; i > angle + 1; i -= stepSize)
            {
                Debug.DrawLine(point + Quaternion.AngleAxis(i, transform.up) * dir, point + Quaternion.AngleAxis(Mathf.Clamp(i - stepSize, angle, 0), transform.up) * dir, color, duration);
            }
        }
        else
        {
            for (float i = 0; i < angle - 1; i += stepSize)
            {
                Debug.DrawLine(point + Quaternion.AngleAxis(i, transform.up) * dir, point + Quaternion.AngleAxis(Mathf.Clamp(i + stepSize, 0, angle), transform.up) * dir, color, duration);
            }
        }
    }

}

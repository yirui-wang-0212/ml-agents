using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Ball3DAgent : Agent
{
    [Header("Specific to Ball3D")]
    public GameObject ball;
    Rigidbody m_BallRb;
    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_BallRb = ball.GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        SetResetParameters();
    }

    // 观察环境：
    // 训练 Shader 项目在此处应为：观察上一次渲染结果
    public override void CollectObservations(VectorSensor sensor)
    {
        // 平台绕Z轴旋转值
        sensor.AddObservation(gameObject.transform.rotation.z);
        // 平台绕X轴旋转值
        sensor.AddObservation(gameObject.transform.rotation.x);
        // 小球与平台的相对位置
        sensor.AddObservation(ball.transform.position - gameObject.transform.position);
        // 小球刚体的速度
        sensor.AddObservation(m_BallRb.velocity);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // 根据策略执行 Action：
        // 训练 Shader 项目在此处应为：调整 Shader 参数并进行渲染

        // 控制平台绕Z轴、X轴旋转的值
        // 用 Mathf.Clamp() 将响应的动作值限制到 -1 到 1
        var actionZ = 2f * Mathf.Clamp(vectorAction[0], -1f, 1f);
        var actionX = 2f * Mathf.Clamp(vectorAction[1], -1f, 1f);
        
        // 将两个 if 的条件去掉训练，发现平台训练过程中比较不稳，抖动较大，因为只要一来值就让平台旋转，可能这里会造成平台一直在调整姿态的过程中
        // 只有在平台Z轴旋转值<0.25f且actionZ>0、或平台Z轴旋转值>0.25f且actionZ<0时才对平台的姿态进行动作，这样就相当于设置了一个缓冲区间，不会让平台不停调整姿态，而是根据小球情况来适当调整姿态。
        // 平台绕 Z 轴旋转响应
        if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
            (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
        {
            gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
        }

        // 平台绕 X 轴旋转响应
        if ((gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
            (gameObject.transform.rotation.x > -0.25f && actionX < 0f))
        {
            gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
        }

        // Reward：
        // 训练 Shader 项目在此处应为：渲染后损失函数

        // 当小球在平台上，掉落或飞出平台，分别进行奖励或惩罚
        if ((ball.transform.position.y - gameObject.transform.position.y) < -2f ||
            Mathf.Abs(ball.transform.position.x - gameObject.transform.position.x) > 3f ||
            Mathf.Abs(ball.transform.position.z - gameObject.transform.position.z) > 3f)
        {
            // 惩罚 -1
            SetReward(-1f);
            // 此次训练结束并重新开始，会调用 AgentReset()
            EndEpisode();
        }
        else
        {
            // 在平台上的时候，每次动作都奖励 0.1
            SetReward(0.1f);
        }
    }

    public override void OnEpisodeBegin()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
            + gameObject.transform.position;
        //Reset the parameters when the Agent is reset.
        SetResetParameters();
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = -Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }

    public void SetBall()
    {
        // 从 Academy 中获取小球的属性（质量、比例）
        //Set the attributes of the ball by fetching the information from the academy
        m_BallRb.mass = m_ResetParams.GetWithDefault("mass", 1.0f);
        var scale = m_ResetParams.GetWithDefault("scale", 1.0f);
        ball.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetResetParameters()
    {
        SetBall();
    }
}

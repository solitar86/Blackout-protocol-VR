using UnityEngine;
public class KeyFrameExample : MonoBehaviour
{
    public AnimationCurve animCurve = null;

    void Start()
    {
        Keyframe[] ks = new Keyframe[2];

        ks[0] = new Keyframe(0f, -5f, 1f, -0.5f); //Create a Keyframe at time 0 with a value of -5f


        ks[1] = new Keyframe(1f, 5f, 0.5f, -1f); //Create a Keyframe at time 1 with a value of 5f

        animCurve = new AnimationCurve(ks);
        animCurve.postWrapMode = WrapMode.PingPong; //Set the WrapMode to PingPong in order to make the GameObject go back and forth
    }

    void Update()
    {
        if (animCurve != null)
            transform.position = new Vector3(animCurve.Evaluate(Time.time), 0.0f, 0.0f);
    }
}

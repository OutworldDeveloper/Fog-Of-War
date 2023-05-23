using UnityEngine;

public class Game : MonoBehaviour
{
    [field: SerializeField] public Transform PlayerCharacter { get; private set; }

    public void MakeNoise(Vector3 position, float distance)
    {
        var hunters = FindObjectsOfType<Hunter>();

        for (int i = 0; i < hunters.Length; i++)
        {
            var hunter = hunters[i];

            if (Vector3.Distance(position, hunter.transform.position) < distance)
            {
                hunter.OnNoiseHeard(position);
            }
        }
    }

}
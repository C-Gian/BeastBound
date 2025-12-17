using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MonsterData activeEnemy;
    public MonsterData playerMonster;

    public Vector3 nextSpawnPosition;
    public bool isReturningFromBattle;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
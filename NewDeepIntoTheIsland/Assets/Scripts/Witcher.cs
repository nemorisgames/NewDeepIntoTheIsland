using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Witcher : MonoBehaviour
{
    public Transform[] spawnPositions;

    Transform player;
    Animator animator;
    Vector3 destination;
    NavMeshAgent agent;

    public float timeHidden = 10f;
    float currentTimeHidden = 0f;
    public float timeUntilChase = 3f;
    float currentTimeUntilChase = 0f;
    public float chaseTime = 4f;
    float currentChaseTime = 0f;

    ArrayList renderizableObjects = new ArrayList();
    ParticleSystem[] particles;

    void Start()
    {
        // Cache agent component and destination
        agent = GetComponent<NavMeshAgent>();
        //destination = agent.destination;
        animator = GetComponent<Animator>();
        player = GameManager.instance.player.transform;

        SkinnedMeshRenderer[] s = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        foreach (SkinnedMeshRenderer sk in s)
            renderizableObjects.Add(sk.gameObject);
        ChangeStatus(GameManager.WitcherStatus.Hidden);
        currentTimeHidden = Time.time + timeHidden;
    }

    void ShowRenderizableObjects(bool show)
    {
        foreach(GameObject g in renderizableObjects)
        {
            g.SetActive(show);
        }
        foreach(ParticleSystem p in particles)
        {
            if (show) p.Play();
            else p.Stop();
        }
    }

    Transform GetClosestSpawnPosition(Transform t, float distanceMin = -1f)
    {
        float distance = Vector3.Distance(spawnPositions[0].position, t.position);
        int indexSelected = 0;
        for(int i = 1; i < spawnPositions.Length; i++)
        {
            if(Vector3.Distance(spawnPositions[i].position, t.position) < distance)
            {
                distance = Vector3.Distance(spawnPositions[i].position, t.position);
                indexSelected = i;
            }
        }

        if (distanceMin >= 0f) {
            if (distance <= distanceMin) return spawnPositions[indexSelected];
            else return null;
        }
        return spawnPositions[indexSelected];
    }

    public void ChangeStatus(GameManager.WitcherStatus w)
    {
        if(w == GameManager.WitcherStatus.Hidden)
        {
            ShowRenderizableObjects(false);
        }
        else
        {
            ShowRenderizableObjects(true);
        }
        /*switch (GameManager.instance.witcherStatus)
        {
            case GameManager.WitcherStatus.Hidden:
                switch (w)
                {
                    case GameManager.WitcherStatus.Watching:
                        currentTimeUntilChase = Time.time + timeUntilChase;
                        break;
                }
                break;
        }*/
        GameManager.instance.witcherStatus = w;
    }

    void Update()
    {
        switch (GameManager.instance.witcherStatus) {
            case GameManager.WitcherStatus.Hidden:
                if(currentTimeHidden <= Time.time)
                {
                    transform.position = GetClosestSpawnPosition(GameManager.instance.player.transform).position;
                    transform.LookAt(GameManager.instance.player.transform);
                    ChangeStatus(GameManager.WitcherStatus.Watching);
                    currentTimeUntilChase = Time.time + timeUntilChase * Random.Range(0.4f, 2f);
                    animator.SetFloat("InputVertical", agent.velocity.magnitude * 0.1f);
                }
                break;
            case GameManager.WitcherStatus.Watching:
                transform.LookAt(GameManager.instance.player.transform);
                if (currentTimeUntilChase <= Time.time)
                {
                    ChangeStatus(GameManager.WitcherStatus.Chasing);
                    currentChaseTime = Time.time + chaseTime * Random.Range(0.4f, 2f);
                    destination = player.position;
                    agent.destination = destination;
                }
                break;
            case GameManager.WitcherStatus.Chasing:
                animator.SetFloat("InputVertical", agent.velocity.magnitude * 0.1f);
                // Update destination if the target moves one unit
                if (Vector3.Distance(destination, player.position) > 1.0f)
                {
                    destination = player.position;
                    agent.destination = destination;
                }
                if(currentChaseTime <= Time.time)
                {
                    destination = GetClosestSpawnPosition(transform).position;
                    agent.destination = destination;
                    ChangeStatus(GameManager.WitcherStatus.Hiding);
                }
                break;
            case GameManager.WitcherStatus.Hiding:
                print(agent.remainingDistance);
                if(agent.remainingDistance < 0.01f)
                {
                    agent.destination = transform.position;
                    animator.SetFloat("InputVertical", agent.velocity.magnitude * 0.1f);
                    currentTimeHidden = Time.time + timeHidden * Random.Range(0.4f, 2f);
                    ChangeStatus(GameManager.WitcherStatus.Hidden);
                }
                break;
        }
    }
}

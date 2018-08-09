using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Witcher : MonoBehaviour
{
    public Transform[] spawnPositions;
    public Transform [] relativeSpawnPositions;
    public bool useWaitTime = true;
    public bool approach = false;
    public float approachSpeed = 0.5f;
    public float runSpeed = 5f;

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
    bool chasePlayer = true;

    ArrayList renderizableObjects = new ArrayList();
    ParticleSystem[] particles;
    public AudioClip[] stepSound;
    public float timeSteps = 0.3f;
    float currentTimeSteps;

    public enum SpawnPos{
        back = 0,
        front = 1,
        closest = 2
    }

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
        Random.InitState(System.DateTime.Now.Minute * System.DateTime.Now.Second * System.DateTime.Now.Hour);
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

    Transform GetSpawnPosition(SpawnPos preferred = SpawnPos.front, float fixedDistance = -1)
    {
        Transform t = null;
        NavMeshHit hitFront, hitBack;
        bool front, back;
        front =  NavMesh.SamplePosition(relativeSpawnPositions[1].position,out hitFront,1f,LayerMask.NameToLayer("Walkable"));
        back = NavMesh.SamplePosition(relativeSpawnPositions[0].position,out hitBack,1f,LayerMask.NameToLayer("Walkable"));
        if (!front && !back)
        {
            //front = preferred == SpawnPos.front;
            //back = preferred != SpawnPos.front;
            preferred = SpawnPos.closest;
        }
        switch(preferred)
        {
            case SpawnPos.back:
                if(back)
                    t = relativeSpawnPositions[0];
                else if(front)
                    t = relativeSpawnPositions[1];
            break;

            case SpawnPos.front:
                if(front)
                    t = relativeSpawnPositions[1];
                else if(back)
                    t = relativeSpawnPositions[0];
            break;

            case SpawnPos.closest:
                t = GetClosestSpawnPosition(GameManager.instance.player.transform);
            break;

        }
        if(fixedDistance != -1 && preferred != SpawnPos.closest)
        {
            fixedDistance = Mathf.Clamp(fixedDistance,5f,float.MaxValue);
            if(preferred == SpawnPos.back)
                fixedDistance *= -1;
            Transform aux = t;
            aux.localPosition = new Vector3(aux.localPosition.x,aux.localPosition.y,fixedDistance);
            t = aux;
        }
        return t;
    }

    public void ChangeStatus(GameManager.WitcherStatus w)
    {
        if(w == GameManager.WitcherStatus.Hidden)
        {
            ShowRenderizableObjects(false);
            GetComponent<AudioSource>().Stop();
        }
        else
        {
            ShowRenderizableObjects(true);
            GetComponent<AudioSource>().Play();
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

    public void StartWatching(SpawnPos spawnPos = SpawnPos.front, Weather weather = Weather.RainNone, float weatherTime = 5f, bool thunder = false, float fixedDistance = -1, bool chasePlayer = true){
        if(GameManager.instance.witcherStatus == GameManager.WitcherStatus.Hidden){
            WeatherManager.Instance.StartNewWeather(weather, weatherTime);
            if(thunder)
                WeatherManager.Instance.ThunderAndFlash();
            //transform.position = GetSpawnPosition(spawnPos, fixedDistance).position;
            GetComponent<NavMeshAgent>().Warp(GetSpawnPosition(spawnPos, fixedDistance).position);
            transform.LookAt(GameManager.instance.player.transform);
            ChangeStatus(GameManager.WitcherStatus.Watching);
            currentTimeUntilChase = Time.time + timeUntilChase * Random.Range(0.4f, 2f);
            this.chasePlayer = chasePlayer;
            animator.SetFloat("InputVertical", agent.velocity.magnitude * 0.1f);
            if(approach){
                agent.speed = approachSpeed;
                destination = player.position;
                agent.destination = destination;
            }
        }
    }

    float currentTimeHiddenAux, currentChaseTimeAux, currentTimeStepsAux, currentTimeUntilChaseAux = 0;

    public void Pause(bool b){
        agent.isStopped = b;
        switch (GameManager.instance.witcherStatus) {
            case GameManager.WitcherStatus.Hidden:
            if(b){
                currentTimeHiddenAux = Time.time;
            }
            else{
                currentTimeHiddenAux = Time.time - currentTimeHiddenAux;
                currentTimeHidden += currentTimeHiddenAux;
            }
            break;

            case GameManager.WitcherStatus.Watching:
            if(b){
                currentTimeUntilChaseAux = Time.time;
            }
            else{
                currentTimeUntilChaseAux = Time.time - currentTimeUntilChaseAux;
                currentTimeUntilChase += currentTimeUntilChaseAux;
            }
            break;

            case GameManager.WitcherStatus.Chasing:
            
            if(b){
                currentChaseTimeAux = Time.time;
                currentTimeStepsAux = Time.time;
            }
            else{
                currentChaseTimeAux = Time.time - currentChaseTimeAux;
                currentChaseTime += currentChaseTimeAux;

                currentTimeStepsAux = Time.time - currentTimeStepsAux;
                currentTimeSteps += currentTimeStepsAux;
            }
            break;
        }
    }
    
    void Update()
    {
        if(GameMenu.Instance.menuActive)
            return;
        //NavMeshHit hitFront, hitBack;
        //Debug.Log("Front: "+NavMesh.SamplePosition(relativeSpawnPositions[1].position,out hitFront,1f,LayerMask.NameToLayer("Walkable"))+" | Back: "+NavMesh.SamplePosition(relativeSpawnPositions[0].position,out hitBack,1f,LayerMask.NameToLayer("Walkable")));
        switch (GameManager.instance.witcherStatus) {
            case GameManager.WitcherStatus.Hidden:
                if(useWaitTime)
                {
                    if(currentTimeHidden <= Time.time)
                        StartWatching();
                    /*transform.position = GetClosestSpawnPosition(GameManager.instance.player.transform).position;
                    transform.position = GetRelativeSpawnPosition(SpawnPos.front).position;
                    transform.LookAt(GameManager.instance.player.transform);
                    ChangeStatus(GameManager.WitcherStatus.Watching);
                    currentTimeUntilChase = Time.time + timeUntilChase * Random.Range(0.4f, 2f);
                    animator.SetFloat("InputVertical", agent.velocity.magnitude * 0.1f);*/
                }
                break;
            case GameManager.WitcherStatus.Watching:
                transform.LookAt(GameManager.instance.player.transform);
                if (currentTimeUntilChase <= Time.time)
                {
                    if(chasePlayer)
                    {
                        ChangeStatus(GameManager.WitcherStatus.Chasing);
                        AudioManager.Instance.PlayVoice(6);
                        ShowMessageOnScreen.Instance.SetText("Oh god, No!");
                        currentChaseTime = Time.time + chaseTime * Random.Range(0.4f, 2f);
                        agent.speed = runSpeed;
                        destination = player.position;
                        agent.destination = destination;
                    }
                    else
                    {
                        agent.destination = transform.position;
                        ChangeStatus(GameManager.WitcherStatus.Hiding);
                    }
                }
                
                break;
            case GameManager.WitcherStatus.Chasing:
                animator.SetFloat("InputVertical", agent.velocity.magnitude * 0.1f);
                // Update destination if the target moves one unit
                if (Vector3.Distance(destination, player.position) > 1.0f)
                {
                    destination = player.position;
                    agent.destination = destination;
                    if (currentTimeSteps <= Time.time)
                    {
                        GetComponent<AudioSource>().PlayOneShot(stepSound[Random.Range(0, stepSound.Length)]);
                        currentTimeSteps = Time.time + timeSteps;
                    }
                }
                if (agent.hasPath && agent.remainingDistance < 1f)
                {
                    GameManager.instance.KillPlayer();
                    agent.destination = transform.position;
                    animator.SetFloat("InputVertical", agent.velocity.magnitude * 0.1f);
                    ChangeStatus(GameManager.WitcherStatus.Hidden);
                    this.enabled = false;
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
                    WeatherManager.Instance.StartNewWeather(Weather.RainNone,5);
                }
                break;
        }
    }
}

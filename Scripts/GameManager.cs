using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Threading;
using TMPro;
using Assets.Scripts;

public class GameManager : MonoBehaviour
{
    //Игрок
    [SerializeField] Transform playerPrefab;                //шаблон игрока
    private PlayerController _playerController;
    private Transform _player;
    [SerializeField] private float playerLivesCount;
    [SerializeField] private Transform palyerRespawnPoint;
    [SerializeField] private float rebirthDelayTime;


    //Враги
    [SerializeField] Transform enemyTypeAPrefab;            //шаблон врага типа A
    [SerializeField] Transform enemyTypeBPrefab;            //шаблон врага типа B
    [SerializeField] Transform enemyTypeCPrefab;            //шаблон врага типа C

    //Пространства активации волн
    [SerializeField] private List<WaveAreaCollider> wavesColliders = new List<WaveAreaCollider>();
    private List<bool> isAreaActivated;

    //Маршруты противников
    [SerializeField] private List<Transform> routesTBank = new List<Transform>();

    //Камера
    [SerializeField] private Camera camera;
    private CameraFollower _cameraFollower;

    //Елементы UI
    [SerializeField] TextMeshProUGUI livesCountValue;
    [SerializeField] TextMeshProUGUI healthValue;
    //Final panel
    [SerializeField] GameObject finalPanel;
    [SerializeField] TextMeshProUGUI finalPanelMessage;

    //Private variables
    private float _timeToRebirth;
    private bool _isPlayerDead;
    private int _enemyCount;
    private bool _isLevelComplete;

    // Start is called before the first frame update
    private void Awake()
    {
        _cameraFollower = camera.GetComponent<CameraFollower>();
        _cameraFollower.SetNewTarget(null);


        isAreaActivated = new List<bool>(wavesColliders.Count);

        for (int i = 0; i < wavesColliders.Count; i++)
        {
            isAreaActivated.Add(false);
        }

        Time.timeScale = 1.0f;
        finalPanel.SetActive(false);

        _isLevelComplete = false;
    }
    private Transform CreatePlayer()
    {
        _isPlayerDead = false;
        Quaternion rotation = new Quaternion(0.70711f, 0, 0, 0.70711f);
        return Instantiate(playerPrefab, palyerRespawnPoint.position, rotation);
    }

    private Transform CreateEnemyTypeA(Transform routeObj)
    {
        if (routeObj != null)
        {
            Route route = routeObj.GetComponent<Route>();
            if (route != null)
            {
                if (route.IsEnable)
                {
                    Quaternion rotation = new Quaternion(0, 0, 0, 1);
                    Transform enemy = Instantiate(enemyTypeAPrefab, route.GetPointByIndex(1).position, rotation);
                    EnemyTypeA tmp = enemy.GetComponent<EnemyTypeA>();

                    if (tmp != null)
                    {
                        tmp.OnDestroy += EnemyOnDestroy;
                        tmp.SetRoute(routeObj);
                    }
                    _enemyCount++;
                    return enemy;
                }
            }
        }
        return null;
    }

    private Transform CreateEnemyTypeB(Transform routeObj)
    {
        if (routeObj != null)
        {
            Route route = routeObj.GetComponent<Route>();
            if (route!=null)
            {
                if (route.IsEnable)
                {
                    Quaternion rotation = new Quaternion(0.70711f, 0, 0, 0.70711f);
                    Transform enemy = Instantiate(enemyTypeBPrefab, route.GetPointByIndex(1).position, rotation);
                    EnemyTypeB tmp = enemy.GetComponent<EnemyTypeB>();

                    if (tmp != null)
                    {
                        tmp.OnDestroy += EnemyOnDestroy;
                        tmp.SetRoute(routeObj);
                    }
                    _enemyCount++;
                    return enemy;
                }
            }   
        }
        return null;   
    }

    private Transform CreateEnemyTypeC(Transform routeObj)
    {
        if (routeObj != null)
        {
            Route route = routeObj.GetComponent<Route>();
            if (route != null)
            {
                if (route.IsEnable)
                {
                    Quaternion rotation = new Quaternion(0, 0, 0, 1);
                    Transform enemy = Instantiate(enemyTypeCPrefab, route.GetPointByIndex(1).position, rotation);
                    EnemyTypeC tmp = enemy.GetComponent<EnemyTypeC>();

                    if (tmp != null)
                    {
                        tmp.OnDestroy += EnemyOnDestroy;
                        tmp.SetRoute(routeObj);
                    }
                    _enemyCount++;
                    return enemy;
                }
            }
        }
        return null;
    }
    private void EnemyOnDestroy(GameObject enemy, string message)
    {
        Debug.Log(message);
        _enemyCount--;
        Debug.Log("Enemy count = " + _enemyCount.ToString());
        enemy.GetComponent<Enemy>().OnDestroy -= EnemyOnDestroy;

    }

    private void GenerateFirstWave()
    {
        if (routesTBank!= null)
        {
            if (routesTBank.Count>=6)
            {
                CreateEnemyTypeB(routesTBank[0]);
                CreateEnemyTypeA(routesTBank[1]);
                CreateEnemyTypeB(routesTBank[2]);
                CreateEnemyTypeB(routesTBank[3]);
                CreateEnemyTypeC(routesTBank[4]);
                CreateEnemyTypeB(routesTBank[5]);
            }
        }
    }

    private void GenerateSecondWave()
    {
        if (routesTBank != null)
        {
            if (routesTBank.Count >= 12)
            {
                CreateEnemyTypeB(routesTBank[3]);
                CreateEnemyTypeB(routesTBank[6]);
                CreateEnemyTypeB(routesTBank[7]);
                CreateEnemyTypeC(routesTBank[8]);
                CreateEnemyTypeA(routesTBank[9]);
                CreateEnemyTypeA(routesTBank[10]);
                CreateEnemyTypeC(routesTBank[11]);
                CreateEnemyTypeC(routesTBank[12]);
            }
        }   
    }

    private void GenerateThirdWave()
    {
        if (routesTBank != null)
        {
            if (routesTBank.Count >= 19)
            {
                CreateEnemyTypeA(routesTBank[9]);
                CreateEnemyTypeA(routesTBank[10]);
                CreateEnemyTypeA(routesTBank[13]);
                CreateEnemyTypeA(routesTBank[14]);
                CreateEnemyTypeB(routesTBank[15]);
                CreateEnemyTypeB(routesTBank[16]);
                CreateEnemyTypeC(routesTBank[17]);
                CreateEnemyTypeC(routesTBank[18]);
                CreateEnemyTypeC(routesTBank[19]);
            }
        }
    }

    private void OnEnable()
    {
        if (wavesColliders.Count!=0)
        {
            foreach (var item in wavesColliders)
            {
                item.OnEnter += PlayerEnteredArea;
                item.OnExit += PlayerExitArea;

            }
        }
    }

    private void OnDisable()
    {
        if (wavesColliders.Count != 0)
        {
            foreach (var item in wavesColliders)
            {
                item.OnEnter -= PlayerEnteredArea;
                item.OnExit -= PlayerExitArea;
            }
        }
    }

    private void PlayerEnteredArea(GameObject sender, GameObject player)
    {
        //Debug.Log("Player Entered to " + sender.name);
        switch (sender.name)
        {
            case "WaveAreaCollider(0)":
                Debug.Log("Player Entered to first area");
                if (!isAreaActivated[0])
                {
                    isAreaActivated[0] = true;
                    GenerateFirstWave();
                }
                break;
            case "WaveAreaCollider(1)":
                Debug.Log("Player Entered to second area");
                if (!isAreaActivated[1])
                {
                    isAreaActivated[1] = true;
                    GenerateSecondWave();
                }
                break;
            case "WaveAreaCollider(2)":
                Debug.Log("Player Entered to second area");
                if (!isAreaActivated[2])
                {
                    isAreaActivated[2] = true;
                    GenerateThirdWave();
                }
                break;
            default:
                break;
        }
    }

    private void PlayerExitArea(GameObject sender, GameObject player)
    {
        Debug.Log("Player Exit From " + sender.name);
    }

    private void PlayerOnDestroy(GameObject player)
    {
        //Debug.Log("Player On Destroy Works");
        _isPlayerDead = true;
        _timeToRebirth = rebirthDelayTime;
        _cameraFollower.SetNewTarget(null);
        playerLivesCount--;
        livesCountValue.text = playerLivesCount.ToString();


        _playerController.OnDestroy -= PlayerOnDestroy;
        _playerController.OnDamage -= PlayerTookDamage;
    }

    private void PlayerTookDamage(GameObject player, int health)
    {
        healthValue.text = _playerController.GetHealthI().ToString();
    }

    public void RestartLevel()
    {
        //Debug.Log("Restart Level");
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
        finalPanel.SetActive(false);
        
    }
    void Start()
    {
        Time.timeScale = 1.0f;

        //Генерация игрока
        _player = CreatePlayer();
        _playerController = _player.GetComponent<PlayerController>();

        //подписка в методе старт, т.к. на момент вызова метода OnEnable объекта не существует
        if (_playerController != null)
        {
            //Debug.Log("_playerController is not null");
            _playerController.OnDestroy += PlayerOnDestroy;
            _playerController.OnDamage += PlayerTookDamage;
            healthValue.text = _playerController.GetHealthI().ToString();
            livesCountValue.text = playerLivesCount.ToString();

        }
        _cameraFollower.SetNewTarget(_player);
    }

    private void Stop()
    {
        //Debug.Log("Stop working");

        finalPanel.SetActive(true);
        Time.timeScale = 0.0f;
        //ShowFinalPanel(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlayerDead)
        {
            //Если игрок мёртв
            if (_timeToRebirth > 0)
            {
                //Задержка возрождения
                _timeToRebirth -= Time.deltaTime;
            }
            else
            {
                if (playerLivesCount > 0)
                {
                    Start();
                }
                else
                {
                    finalPanelMessage.text = "Game Over";
                    finalPanelMessage.color = new Color(255, 0, 0);
                    Stop();
                }

            }
        }
        else
        {
            //Если игрок жив
            //Ожидание ввода клавиши Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Escape button KeyCode");
                finalPanelMessage.text = "Game Stoped";
                finalPanelMessage.color = new Color(115, 111, 111);
                Stop();
            }

            if (!_isLevelComplete)
            {
                if (_enemyCount <= 0)
                {
                    //Если количество врагов 0 или меньше, и все волны активированы
                    //Тогда уровень пройден (см.блок else)
                    bool isAllWavesActivated = true;

                    for (int i = 0; i < isAreaActivated.Count; i++)
                    {
                        if (!isAreaActivated[i])
                        {
                            isAllWavesActivated = false;
                            break;
                        }
                    }

                    if (isAllWavesActivated)
                    {
                        _isLevelComplete = true;
                        _timeToRebirth = rebirthDelayTime;
                    }
                }
            }
            else
            {
                //Если уровень пройден
                if (_timeToRebirth > 0)
                {
                    //Задержка активации финальной панели
                    _timeToRebirth -= Time.deltaTime;
                }
                else
                {
                    finalPanelMessage.text = "Game Over";
                    finalPanelMessage.color = new Color(0, 255, 0);
                    Stop();
                }
            }
        }
    }

    public void QuitGame()
    {
        //Debug.Log("Application Quit");
        Application.Quit();
    }
}

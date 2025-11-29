using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public abstract class Enemy : MonoBehaviour, IDamagebale
{
    public Action<GameObject, string> OnDestroy;
    //public Action<GameObject, int> OnDamage;

    public bool isCircleRoute;
    //public bool isUseRoutePointsBank;
    public float collisionDamageMultiplayer;
    public float collisionImpulseMultiplayer;
    public float health;
    public float speed;
    public float targetOffset;
    public string targetTag;
    [SerializeField] protected Transform routeObj;
    //public List<Transform> routePointsBank = new List<Transform>();
    public float knockoutTime;
    [SerializeField] protected Transform dyingball;

    protected bool _isAttackmode;
    protected Route _route;
    public List<Transform> _routePoints;
    protected int _nextRouteNumber;
    protected bool _isPositiveRouteChange;


    protected Rigidbody _rb;
    protected bool _hasRigidbody;
    protected Quaternion _targetRotation;

    protected float _knockoutRemainder;

    protected GameObject target;
    protected PlayerController playerController;

    protected bool _isDeathActivated;
    protected float _dieDelay;

    protected float _reloadTime;

    protected abstract void Attack();
    //protected abstract void Bump(Collision collision);
    public abstract void Die();
    protected abstract void DebugHelathChange();

    protected void Move()
    {
        //Debug.Log("Enemy Move Working");
        if (_routePoints == null)
        {
            Debug.Log("_routePoints is null");
        }
        else
        {
            if (_routePoints.Count == 0)
            {
                Debug.Log("_routePoints is empty");
            }
            else
            {
                //Debug.Log("Coordinates " + transform.position + " Direct position" + routePoints[_nextRouteNumber].position);
                //≈сли маршрутных точек больше двух, тогда объект двигаетс€, потому что, если точек меньше двух объект передвинетс€ только до первой точки
                if (_routePoints.Count > 2)
                {
                    //ќпредел€етс€ разница в координатах, т.к. если сранивать просто по координатам, возникают сбои из-за особенностей хранени€ float в пам€ти
                    float differenceX = transform.position.x - _routePoints[_nextRouteNumber].position.x;
                    float differenceZ = transform.position.z - _routePoints[_nextRouteNumber].position.z;

                    //ќпредел€етс€ достигнута-ли точка, т.е. разница в координатах меньше необходимой точности
                    bool isXunDestinated = Math.Abs(differenceX) > 0.1f;
                    bool isZunDestinated = Math.Abs(differenceZ) > 0.1f;
                    bool isUndestinated = isXunDestinated || isZunDestinated;


                    if (isUndestinated)
                    {
                        //≈сли точка не достигнута, то производитс€ движение

                        //Debug.Log("Moving");
                        //Debug.Log("Moving to " + _nextRouteNumber.ToString());

                        MoveTo(_routePoints[_nextRouteNumber]);
                    }
                    else
                    {
                        //≈сли точка достигнута, устанавливаетс€ нова€ точка

                        //string debug = "Change|isUndestinated=" + isUndestinated + "|isXunDestinated=" + isXunDestinated + "|isZunDestinated=" + isZunDestinated;
                        //debug += "|_nextRouteNumber" + _nextRouteNumber.ToString();
                        //Debug.Log(debug)

                        //¬ зависимости от режима перемещени€( по кругу или нет) алгоритм установки новой точки разный
                        if (isCircleRoute)
                        {
                            //≈сли перемещаетс€ по кругу, то при достижении последней точки в List<Transform> routePoints устанавливаетс€ нулева€ точка из этого списка
                            if (_nextRouteNumber == _routePoints.Count - 1)
                            {
                                _nextRouteNumber = 0;
                                //Debug.Log("Circle Moving New Lap " + _nextRouteNumber.ToString());
                            }
                            else
                            {
                                _nextRouteNumber++;
                                //Debug.Log("Circle Moving change point " + _nextRouteNumber.ToString());
                            }
                        }
                        else
                        {
                            //≈сли перемещение не по кругу определ€етс€ в каком направлении двигаетс€ объект, по возрастанию, или по убыванию
                            if (_isPositiveRouteChange)
                            {
                                if (_nextRouteNumber == _routePoints.Count - 1)
                                {
                                    //≈сли объект двигаетс€ по возрастанию и достиг последней точки в List<Transform> routePoints
                                    //измен€етс€ направление движени€ и задаЄтс€ предыдуща€ точка

                                    _isPositiveRouteChange = false;
                                    _nextRouteNumber--;
                                    //Debug.Log("UnCircle Moving turn back " + _nextRouteNumber.ToString());
                                }
                                else
                                {
                                    //≈сли объект двигаетс€ по возрастанию и не достиг последней точки в List<Transform> routePoints устанавливаетс€ следующа€ точка из списка
                                    _nextRouteNumber++;
                                    //Debug.Log("UnCircle Moving change point fwd " + _nextRouteNumber.ToString());
                                }
                            }
                            else
                            {
                                if (_nextRouteNumber == 0)
                                {
                                    //≈сли объект двигаетс€ по убыванию и достиг нулевой точки в List<Transform> routePoints
                                    //измен€етс€ направление движени€ и задаЄтс€ следующа€ точка

                                    _isPositiveRouteChange = true;
                                    _nextRouteNumber++;
                                    //Debug.Log("UnCircle Moving turn fwd " + _nextRouteNumber.ToString());
                                }
                                else
                                {
                                    //≈сли объект двигаетс€ по убыванию и не достиг нулевой точки в List<Transform> routePoints устанавливаетс€ предыдуща€ точка из списка
                                    _nextRouteNumber--;
                                    //Debug.Log("UnCircle Moving change point back " + _nextRouteNumber.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }   
    }

    protected abstract void MoveTo(Transform target);

    protected void OnCollisionEnter(Collision collision)
    {
        if (_isAttackmode)
        {
            if (collision.gameObject.CompareTag(targetTag))
            {
                IDamagebale targetFordamage = collision.gameObject.GetComponent<IDamagebale>();

                if (targetFordamage != null)
                {
                    targetFordamage.TakeDamage(collision.impulse * collisionDamageMultiplayer);
                }
                TakeDamage(collision.impulse * collisionDamageMultiplayer * 0.2f);
            }
        }

        if (collision.gameObject.CompareTag("Asteroid"))
        {
            //Debug.Log("EnemyA Asteroid collision "+ collision.impulse + " Contact=" + collision.GetContact(0).point + " Transform"+transform.position);
            TakeDamage(collision.impulse * collisionDamageMultiplayer * 1f);

            if (_knockoutRemainder <= 0)
            {
                _knockoutRemainder = knockoutTime;
            }

            if (collision.impulse.x <= 0.01f && collision.impulse.z <= 0.01f)
            {
                float deltaX = transform.position.x - collision.GetContact(0).point.x;
                float deltaZ = transform.position.z - collision.GetContact(0).point.z;
                Vector2 bumpDirection = new Vector2(deltaX, deltaZ);
                //Debug.Log("DeltaX=" + (-bumpDirection.normalized.x * collisionImpulseMultiplayer).ToString() + "|DeltaZ=" + (-bumpDirection.normalized.y * collisionImpulseMultiplayer).ToString());
                _rb.AddForce(new Vector3(bumpDirection.normalized.x * collisionImpulseMultiplayer, 0.0f, bumpDirection.normalized.y * collisionImpulseMultiplayer), ForceMode.Impulse);
            }
            else
            {
                //Debug.Log("EnemyA Asteroid collision " + collision.impulse + " Velocity=" + _rb.velocity);
                _rb.AddForce(new Vector3(collision.impulse.x * collisionImpulseMultiplayer, 0.0f, collision.impulse.z * collisionImpulseMultiplayer), ForceMode.Impulse);
            }
        }

        
        
    }

    protected void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Enemy Type A TriggerEnter");


        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy Type A Player TriggerEnter");
            target = other.gameObject;
            _isAttackmode = true;

            playerController = other.GetComponent<PlayerController>();
            if (playerController!=null)
            {
                playerController.OnDestroy += TargetOnDestroy;
            }

        }
    }

    protected void OnTriggerExit(Collider other)
    {
        Debug.Log("Enemy Type A TriggerExit");
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy Type A Player TriggerExit");
            _isAttackmode = false;
            
            if (playerController != null)
            {
                playerController.OnDestroy -= TargetOnDestroy;
            }

            target = null;
            playerController = null;
        }
    }

    public void TakeDamage(Vector3 damage)
    {
        float result = (float)Math.Sqrt(damage.x * damage.x + damage.y * damage.y);
        health -= result;
        DebugHelathChange();
    }

    public void TakeScalarDamage(float damage)
    {
        health -= damage;
        DebugHelathChange();
    }

    protected void TargetOnDestroy(GameObject eventActor)
    {
        if (target = eventActor)
        {
            _isAttackmode = false;
            if (playerController!=null)
            {
                playerController.OnDestroy -= TargetOnDestroy;
            }   
        }
    }


    public void SetRoute(Transform routeOBJ)
    {
        if (routeOBJ != null)
        {
            routeObj = routeOBJ;
            Route route = routeOBJ.GetComponent<Route>();
            if (route.IsEnable)
            {
                _routePoints = new List<Transform>(route.RoutePointsCount);
                for (int i = 0; i < route.RoutePointsCount; i++)
                {
                    _routePoints.Add(routeOBJ.GetChild(i).transform);
                }
            }
        }
    }
    protected abstract void StabilizeXRotation();

    // Start is called before the first frame update
    protected void Start()
    {
        _isAttackmode = false;

        try
        {
            _route = routeObj.GetComponent<Route>();
        }
        catch (Exception)
        {

            _route = null;
        }

        _nextRouteNumber = 0;
        _isPositiveRouteChange = true;

        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _hasRigidbody = true;
            _rb.constraints = RigidbodyConstraints.FreezePositionY;                             //наложение перемещени€ вращени€ по оси Y, чтобы не смещалс€ по врем€ столкновений
            _rb.constraints = RigidbodyConstraints.FreezeRotationZ;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX;
        }
        _targetRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        _reloadTime = -0.01f;

        _knockoutRemainder = -0.01f;

        Debug.Log("Enemy rotation " + transform.rotation);
    }

    // Update is called once per frame
    protected abstract void Update();
}

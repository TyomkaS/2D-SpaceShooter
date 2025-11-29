using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public class EnemyTypeA : Enemy
{
    protected float _xStep;
    protected float _zStep;
    protected override void Attack()
    {
        if (target != null)
        {
            //float deltaX = transform.position.x - target.transform.position.x;
            //float deltaZ = transform.position.z - target.transform.position.z;
            //float distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaZ, 2));

            MoveTo(target.transform);
        }
        else
        {
            _isAttackmode = false;
        }
    }

    public override void Die()
    {
        if (!_isDeathActivated)
        {
            Transform ballObj = Instantiate(dyingball, transform.position, Quaternion.identity);
            DamageBall ball = ballObj.GetComponent<DamageBall>();

            ball.maximumScale = 10;

            ball.followTo = transform;
            _dieDelay = (ball.maximumScale / ball.growthSpeed) * 0.25f;
            _isDeathActivated = true;
        }
        else
        {
            if (_dieDelay > 0)
            {
                _dieDelay -= Time.deltaTime;
            }
            else
            {
                OnDestroy?.Invoke(this.gameObject,"Type A");
                Destroy(this.gameObject);
            }
        }
    }

    protected override void DebugHelathChange()
    {
        Debug.Log("Enemy Type A health=" + health.ToString());
    }

    protected override void MoveTo(Transform target)
    {
        if (target != null)
        {
            //Так сделано, чтобы не использовать Lerp, т.к. с Lerp скорость движения
            //объекта уменьшается при приближении к точке назначения

            //Подготовка к перемещению, расчитывается _xStep и _zStep 
            PreparTraectoryTo(target);

            float targetX = transform.position.x - _xStep;
            float targetZ = transform.position.z - _zStep;

            Vector3 targetposition = new Vector3(targetX, 2.5f, targetZ);
            transform.position = targetposition;
        }
    }

    private void PreparTraectoryTo(Transform target)
    {
        float distanceX = transform.position.x - target.position.x;
        float distanceZ = transform.position.z - target.position.z;
        float distance = (float)Math.Sqrt(distanceX * distanceX + distanceZ * distanceZ);

        int stepCount = (int)((distance / speed) / Time.deltaTime);

        //Защита от получения значения бесконечности, которое вызывает ошибку
        if (stepCount!=0)
        {
            _xStep = distanceX / stepCount;
            _zStep = distanceZ / stepCount;
        }
       
    }

    protected override void StabilizeXRotation()
    {
        _rb.angularVelocity = Vector3.zero;

        if ((Math.Abs(transform.eulerAngles.x)) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime);
            Debug.Log("Enemy type A stabilze rotation");
        }
    }

    protected override void Update()
    {
        if (transform.position.y != 2.5f)
        {
            transform.position = new Vector3(transform.position.x, 2.5f, transform.position.z);
        }

        if (health > 0)
        {
            if (_knockoutRemainder < 0)
            {
                if ((Math.Abs(transform.eulerAngles.x)) < 0.5f)
                {
                    if (!_isAttackmode)
                    {
                        Move();
                    }
                    else
                    {
                        Attack();
                    }

                    if (_reloadTime > 0)
                    {
                        _reloadTime -= Time.deltaTime;
                    }
                }
                else
                {
                    StabilizeXRotation();
                }
            }
            else
            {
                _knockoutRemainder -= Time.deltaTime;
                //Debug.Log("Enemy Tipe B _knockoutRemainder=" + _knockoutRemainder.ToString());
            }
        }
        else
        { Die(); }
    }
}

/*{
    
    
    public float speed;
    public float health;
    public bool isCircleRoute;
    public bool isUseRoutePointsBank;
    [SerializeField] GameObject routeObj;
    [SerializeField] private List<Transform> routePointsBank = new List<Transform>();
    [SerializeField] private float knockoutTime;

    private bool _isAttackmode;
    private Route _route;
    private List<Transform> _routePoints;
    private int _nextRouteNumber;
    private bool _isPositiveRouteChange;
    private float _xStep;
    private float _zStep;

    private Rigidbody _rb;
    private bool _hasRigidbody;
    private Quaternion _targetRotation;

    private float _knockoutRemainder;

    private GameObject target;

    private void Awake()
    {
        
    }
    public void Attack()
    {
        Debug.Log("Attacking");
        //throw new System.NotImplementedException();
        if (target!=null)
        {
            float deltaX = transform.position.x - target.transform.position.x;
            float deltaZ = transform.position.z - target.transform.position.z;
            float distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaZ, 2));

            //Debug.Log("Distance to target=" + distance.ToString());
            if (distance > 7.5f)
            {
                Debug.Log("Move for attack distance=" + distance.ToString());
                MoveTo(target.transform);
            }
        }
    }

    public void Die()
    {
        Debug.Log("Diying");
        //throw new System.NotImplementedException();
    }

    public void Move()
    {
        if (_routePoints == null)
        {
            Debug.Log("_routePoints is null");
        }
        else
        {
            if (_routePoints.Count==0)
            {
                Debug.Log("_routePoints is empty");
            }
        }
        //Debug.Log("Coordinates " + transform.position + " Direct position" + routePoints[_nextRouteNumber].position);
        //Если маршрутных точек больше двух, тогда объект двигается, потому что, если точек меньше двух объект передвинется только до первой точки
        if (_routePoints.Count>2)
        {
            //Определяется разница в координатах, т.к. если сранивать просто по координатам, возникают сбои из-за особенностей хранения float в памяти
            float differenceX = transform.position.x - _routePoints[_nextRouteNumber].position.x;
            float differenceZ = transform.position.z - _routePoints[_nextRouteNumber].position.z;

            //Определяется достигнута-ли точка, т.е. разница в координатах меньше необходимой точности
            bool isXunDestinated = Math.Abs(differenceX) > 0.1f;
            bool isZunDestinated = Math.Abs(differenceZ) > 0.1f;
            bool isUndestinated = isXunDestinated || isZunDestinated;


            if (isUndestinated)
            {
                //Если точка не достигнута, то производится движение

                //Debug.Log("Moving");
                //Debug.Log("Moving to " + _nextRouteNumber.ToString());

                MoveTo(_routePoints[_nextRouteNumber]);
            }
            else
            {
                //Если точка достигнута, устанавливается новая точка

                //string debug = "Change|isUndestinated=" + isUndestinated + "|isXunDestinated=" + isXunDestinated + "|isZunDestinated=" + isZunDestinated;
                //debug += "|_nextRouteNumber" + _nextRouteNumber.ToString();
                //Debug.Log(debug)

                //В зависимости от режима перемещения( по кругу или нет) алгоритм установки новой точки разный
                if (isCircleRoute)
                {
                    //Если перемещается по кругу, то при достижении последней точки в List<Transform> routePoints устанавливается нулевая точка из этого списка
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
                    //Если перемещение не по кругу определяется в каком направлении двигается объект, по возрастанию, или по убыванию
                    if (_isPositiveRouteChange)
                    {
                        if (_nextRouteNumber == _routePoints.Count - 1)
                        {
                            //Если объект двигается по возрастанию и достиг последней точки в List<Transform> routePoints
                            //изменяется направление движения и задаётся предыдущая точка

                            _isPositiveRouteChange = false;
                            _nextRouteNumber--;
                            //Debug.Log("UnCircle Moving turn back " + _nextRouteNumber.ToString());
                        }
                        else
                        {
                            //Если объект двигается по возрастанию и не достиг последней точки в List<Transform> routePoints устанавливается следующая точка из списка
                            _nextRouteNumber++;
                            //Debug.Log("UnCircle Moving change point fwd " + _nextRouteNumber.ToString());
                        }
                    }
                    else
                    {
                        if (_nextRouteNumber == 0)
                        {
                            //Если объект двигается по убыванию и достиг нулевой точки в List<Transform> routePoints
                            //изменяется направление движения и задаётся следующая точка

                            _isPositiveRouteChange = true;
                            _nextRouteNumber++;
                            //Debug.Log("UnCircle Moving turn fwd " + _nextRouteNumber.ToString());
                        }
                        else
                        {
                            //Если объект двигается по убыванию и не достиг нулевой точки в List<Transform> routePoints устанавливается предыдущая точка из списка
                            _nextRouteNumber--;
                            //Debug.Log("UnCircle Moving change point back " + _nextRouteNumber.ToString());
                        }
                    }
                }
            }
        }
    }

    private void MoveTo(Transform target)
    {
        if (target!=null)
        {
            //Так сделано, чтобы не использовать Lerp, т.к. с Lerp скорость движения
            //объекта уменьшается при приближении к точке назначения

            //Подготовка к перемещению, расчитывается _xStep и _zStep 
            PreparTraectoryTo(target);

            float targetX = transform.position.x - _xStep;
            float targetZ = transform.position.z - _zStep;
            Vector3 targetposition = new Vector3(targetX, 2.5f, targetZ);
            transform.position = targetposition;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Enemy Type A Collision");
        _knockoutRemainder = knockoutTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Enemy Type A TriggerEnter");


        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy Type A Player TriggerEnter");
            target = other.gameObject;
            _isAttackmode = true;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Enemy Type A TriggerExit");
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy Type A Player TriggerExit");
            _isAttackmode = false;
            target = null;
        }
    }

    private void PreparTraectoryTo(Transform target)
    {
        float distanceX = transform.position.x - target.position.x;
        float distanceZ = transform.position.z - target.position.z;
        float distance = (float)Math.Sqrt(distanceX * distanceX + distanceZ * distanceZ);

        int stepCount = (int)((distance / speed)/Time.deltaTime);

        _xStep = distanceX / stepCount;
        _zStep = distanceZ / stepCount;
    }

    public void TakeDamage(Vector3 damage)
    {
        Debug.Log("Taking Damage");
        //throw new System.NotImplementedException();
    }

    public void SetRoute(Route route)
    {
        if (route!=null)
        {
            if (route.IsEnable)
            {
                for (int i = 0; i < _route.RoutePointsCount; i++)
                {
                    _routePoints.Add(_route.GetPointByIndex(i));
                }
            }
        }
        
    }

    private void StabilizeXRotation()
    {
        _rb.angularVelocity = Vector3.zero;

        if ((Math.Abs(transform.eulerAngles.x)) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime);
            //Debug.Log("StabilizeXRotation works.|BeginAngle="+ biginangle + " XAngle=" + transform.eulerAngles.x.ToString()+"|angle=" + angle.ToString());
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        _isAttackmode = false;

        try
        {
            _route=routeObj.GetComponent<Route>();
        }
        catch (Exception)
        {

            throw;
        }


        _routePoints = new List<Transform>();
        if (isUseRoutePointsBank)
        {
            if (routePointsBank.Count > 2)
            {
                for (int i = 0; i < routePointsBank.Count; i++)
                {
                    _routePoints.Add(routePointsBank[i]);
                }
            }
            else
            {
                SetRoute(_route);
            }
        }
        else
        {
            SetRoute(_route);
        }
        _nextRouteNumber = 0;
        _isPositiveRouteChange = true;

        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _hasRigidbody = true;
            _rb.constraints = RigidbodyConstraints.FreezePositionY;                             //наложение перемещения вращения по оси Y, чтобы не смещался по время столкновений
            _rb.constraints = RigidbodyConstraints.FreezeRotationZ;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX;
        }
        _targetRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y != 2.5f)
        {
            transform.position = new Vector3(transform.position.x, 2.5f, transform.position.z);
        }

        if (_knockoutRemainder < 0)
        {
            if ((Math.Abs(transform.eulerAngles.x)) < 0.5f)
            {
                if (!_isAttackmode)
                {
                    Move();
                }
                else
                {
                    Attack();
                }
            }
            else
            {
                StabilizeXRotation();
            }
        }
        else
        {
            _knockoutRemainder -= Time.deltaTime;
            Debug.Log("Enemy Tipe B _knockoutКemainder=" + _knockoutRemainder.ToString());
        }
        
    }
}*/

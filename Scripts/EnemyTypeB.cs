using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;


public class EnemyTypeB : Enemy
{
    [SerializeField] private Transform bullet;
    [SerializeField] private Transform shootStartPosition;
    [SerializeField] private float reloadTime;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float bulletSpeed;

    protected override void Attack()
    {
        if (target != null)
        {
            float deltaX = transform.position.x - target.transform.position.x;
            float deltaZ = transform.position.z - target.transform.position.z;
            float distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaZ, 2));

            if (distance > targetOffset)
            {
                //Так сделано, чтобы объект не двигался вперёд, если distance ближе targetOffset
                MoveTo(target.transform);
            }
            else
            {
                //Этот кусок кода взят из метода MoveTo, чтобы повернуть объект, если цель ближе targetOffset
                float angle = GetHorizAngle(target.transform);

                if (Math.Abs(angle) > 0.5)
                {
                    if (angle > 0)
                    {
                        transform.Rotate(0.0f, turnSpeed * Time.deltaTime, 0.0f, Space.World);
                    }
                    else
                    {
                        transform.Rotate(0.0f, -turnSpeed * Time.deltaTime, 0.0f, Space.World);
                    }
                }
            }

            Shoot();
            //Debug.Log("Enemy B is Attacking");
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

            ball.maximumScale = 20;

            ball.followTo = transform;
            _dieDelay = (ball.maximumScale / ball.growthSpeed) * 0.1f;
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
                OnDestroy?.Invoke(this.gameObject, "Type B");
                Destroy(this.gameObject);
            }
        }
    }

    protected override void DebugHelathChange()
    {
        Debug.Log("Enemy Type B health=" + health.ToString());
    }

    private float GetHorizAngle(Transform targetobj)
    {
        //Определяет горизонтальный угол (в плоскости XZ) между целью и текущим объектом
        Vector2 target = new Vector2(targetobj.position.x, targetobj.position.z);
        Vector2 obj = new Vector2(transform.position.x, transform.position.z);
        Vector2 frw = new Vector2(transform.up.x, transform.up.z);
        Vector2 direction = target - obj;

        //Вспомогательные векторы для ориентации
        Vector2 right = new Vector2(transform.right.x, transform.right.z);
        Vector2 left = new Vector2(-transform.right.x, -transform.right.z);

        //Измеренные углы
        float fangle = Vector2.Angle(frw, direction);

        //Вспомогтаельные углы, измеренные от векторов right и left
        float rangle = Vector2.Angle(right, direction);
        float langle = Vector2.Angle(left, direction);

        //Debug.Log("Angle" + fangle.ToString() + "|FRW=" + frw +"|RAngle=" + rangle.ToString() + "|Right=" + right + "|LAngle=" + langle.ToString() + "|Left=" + left);

        //ЛОГИКА
        //Если fangle равен 180 или 0, сл-но объект сзади или спереди, и возвращается значение fangle
        //Если rangle < langle, сл-но объект с правой стороны и возвращается fangle
        //Иначе объект находится с левой стороны, и возвращается fangle+180
        //Таким образом значения от 0.01 до 179.99 означают, что объект справа,
        //А значения от -0.01 до -179.99 означают, что объект слева 
        if (fangle != 180)
        {
            if (fangle != 0)
            {
                if (rangle < langle)
                {
                    return fangle;
                }
                else
                {
                    return fangle * -1;
                }
            }
            else
            {
                return fangle;
            }
        }
        else
        {
            return fangle;
        }
    }

    protected override void MoveTo(Transform target)
    {
        float angle = GetHorizAngle(target);

        if (Math.Abs(angle) < 1.5f)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
        else
        {
            if (angle > 0)
            {
                transform.Rotate(0.0f, turnSpeed * Time.deltaTime, 0.0f, Space.World);
            }
            else
            {
                transform.Rotate(0.0f, -turnSpeed * Time.deltaTime, 0.0f, Space.World);
            }
        }

    }

    private void Shoot()
    {
        //Debug.Log("Shoot works");
        if (_reloadTime <= 0)
        {
            Vector3 startposition = new Vector3(shootStartPosition.position.x, shootStartPosition.position.y, shootStartPosition.position.z);
            Transform localbullet = Instantiate(bullet, startposition, Quaternion.identity);
            localbullet.rotation = transform.rotation;
            Bullet pb = localbullet.GetComponent<Bullet>();
            //transform.TransformDirection(Vector3.up) - получение направления игрока, чтобы пули стреляли прямо перед игроком
            //Debug.Log("Shoot transform fwd" + transform.TransformDirection(Vector3.up));
            pb.SetVelocity(transform.TransformDirection(Vector3.up * bulletSpeed));

            _reloadTime = reloadTime;
        }

    }

    protected override void StabilizeXRotation()
    {
        _rb.angularVelocity = Vector3.zero;

        if ((Math.Abs(transform.eulerAngles.x - 90)) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime);
            //Debug.Log("StabilizeXRotation works.|BeginAngle="+ biginangle + " XAngle=" + transform.eulerAngles.x.ToString()+"|angle=" + angle.ToString());
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
                if ((Math.Abs(transform.eulerAngles.x) - 90) < 0.5f)
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
/*public class EnemyTypeB : MonoBehaviour
{
    public List<Transform> routePoints = new List<Transform>();
    public bool isCircleRoute;
    public float movingFwdSpeed;
    public float turnSpeed;
    public float health;

    [SerializeField] private float knockoutTime;

    private bool _isAttackmode;
    private int _nextRouteNumber;
    private int _routePointsCount;
    private bool _isPositiveRouteChange;
    private Transform _target;

    private Rigidbody _rb;
    private bool _hasRigidbody;
    private Quaternion _targetRotation;

    private float _knockoutRemainder;
    private void Awake()
    {

    }
    public void Attack()
    {
        Debug.Log("Attack");
        //throw new System.NotImplementedException();
    }

    public void Die()
    {
        //throw new System.NotImplementedException();
    }

    private void FixedUpdate()
    {
        Ray up, right, left;
        up = new Ray(transform.position, transform.TransformDirection(Vector3.up));
        right = new Ray(transform.position, transform.TransformDirection(Vector3.right));
        left = new Ray(transform.position, transform.TransformDirection(-Vector3.right));

        Debug.DrawRay(up.origin, up.direction * 8, Color.green);
        Debug.DrawRay(right.origin, right.direction * 8, Color.yellow);
        Debug.DrawRay(left.origin, left.direction * 8, Color.red);
    }

    private float GetHorizAngle(Transform targetobj)
    {
        //Определяет горизонтальный угол (в плоскости XZ) между целью и текущим объектом
        Vector2 target = new Vector2(targetobj.position.x, targetobj.position.z);
        Vector2 obj = new Vector2(transform.position.x, transform.position.z);
        Vector2 frw = new Vector2(transform.up.x, transform.up.z);
        Vector2 direction = target - obj;

        //Вспомогательные векторы для ориентации
        Vector2 right = new Vector2(transform.right.x, transform.right.z);
        Vector2 left = new Vector2(-transform.right.x, -transform.right.z);

        //Измеренные углы
        float fangle = Vector2.Angle(frw, direction);

        //Вспомогтаельные углы, измеренные от векторов right и left
        float rangle = Vector2.Angle(right, direction);
        float langle = Vector2.Angle(left, direction);

        //Debug.Log("Angle" + fangle.ToString() + "|FRW=" + frw +"|RAngle=" + rangle.ToString() + "|Right=" + right + "|LAngle=" + langle.ToString() + "|Left=" + left);

        //ЛОГИКА
        //Если fangle равен 180 или 0, сл-но объект сзади или спереди, и возвращается значение fangle
        //Если rangle < langle, сл-но объект с правой стороны и возвращается fangle
        //Иначе объект находится с левой стороны, и возвращается fangle+180
        //Таким образом значения от 0.01 до 179.99 означают, что объект справа,
        //А значения от -0.01 до -179.99 означают, что объект слева 
        if (fangle != 180)
        {
            if (fangle != 0)
            {
                if (rangle < langle)
                {
                    return fangle;
                }
                else
                {
                    return fangle * -1;
                }
            }
            else
            {
                return fangle;
            }
        }
        else
        {
            return fangle;
        }
    }

    public void Move()
    {
        if (_routePointsCount > 2)
        {
            
            float angle = GetHorizAngle(_target);

            if (Math.Abs(angle)<0.5)
            {
                //Определяется разница в координатах, т.к. если сранивать просто по координатам, возникают сбои из-за особенностей хранения float в памяти
                float differenceX = transform.position.x - routePoints[_nextRouteNumber].position.x;
                float differenceZ = transform.position.z - routePoints[_nextRouteNumber].position.z;

                //Определяется достигнута-ли точка, т.е. разница в координатах меньше необходимой точности
                bool isXunDestinated = Math.Abs(differenceX) > 0.1f;
                bool isZunDestinated = Math.Abs(differenceZ) > 0.1f;
                bool isUndestinated = isXunDestinated || isZunDestinated;


                if (isUndestinated)
                {
                    //Если точка не достигнута, то производится движение

                    //Debug.Log("Moving");
                    //Debug.Log("Moving to " + _nextRouteNumber.ToString());

                    transform.Translate(Vector3.up * movingFwdSpeed * Time.deltaTime);
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
                        if (_nextRouteNumber == _routePointsCount - 1)
                        {
                            _nextRouteNumber = 0;
                            _target = routePoints[_nextRouteNumber];
                            //Debug.Log("Circle Moving New Lap " + _nextRouteNumber.ToString());
                        }
                        else
                        {
                            _nextRouteNumber++;
                            _target = routePoints[_nextRouteNumber];
                            //Debug.Log("Circle Moving change point " + _nextRouteNumber.ToString());
                        }
                    }
                    else
                    {
                        //Если перемещение не по кругу определяется в каком направлении двигается объект, по возрастанию, или по убыванию
                        if (_isPositiveRouteChange)
                        {
                            if (_nextRouteNumber == _routePointsCount - 1)
                            {
                                //Если объект двигается по возрастанию и достиг последней точки в List<Transform> routePoints
                                //изменяется направление движения и задаётся предыдущая точка

                                _isPositiveRouteChange = false;
                                _nextRouteNumber--;
                                _target = routePoints[_nextRouteNumber];
                                //Debug.Log("UnCircle Moving turn back " + _nextRouteNumber.ToString());
                            }
                            else
                            {
                                //Если объект двигается по возрастанию и не достиг последней точки в List<Transform> routePoints устанавливается следующая точка из списка
                                _nextRouteNumber++;
                                _target = routePoints[_nextRouteNumber];
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
                                _target = routePoints[_nextRouteNumber];
                                //Debug.Log("UnCircle Moving turn fwd " + _nextRouteNumber.ToString());
                            }
                            else
                            {
                                //Если объект двигается по убыванию и не достиг нулевой точки в List<Transform> routePoints устанавливается предыдущая точка из списка
                                _nextRouteNumber--;
                                _target = routePoints[_nextRouteNumber];
                                //Debug.Log("UnCircle Moving change point back " + _nextRouteNumber.ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                //Debug.Log("Turning works");
                //Если angle > 0 поворачивает в право, иначе в лево
                if (angle > 0)
                {
                    transform.Rotate(0.0f, turnSpeed * Time.deltaTime, 0.0f, Space.World);
                }
                else
                {
                    transform.Rotate(0.0f, -turnSpeed * Time.deltaTime, 0.0f, Space.World);
                }
            }

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Enemy Type B Collision");
        _knockoutRemainder = knockoutTime;
    }

    public void TakeDamage(Vector3 damage)
    {
        //throw new System.NotImplementedException();
    }

    private void StabilizeXRotation()
    {
        _rb.angularVelocity = Vector3.zero;

        if ((Math.Abs(transform.eulerAngles.x - 90)) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime);
            //Debug.Log("StabilizeXRotation works.|BeginAngle="+ biginangle + " XAngle=" + transform.eulerAngles.x.ToString()+"|angle=" + angle.ToString());
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        _isAttackmode = false;

        if (routePoints.Count > 2)
        {
            _routePointsCount = routePoints.Count;
            _nextRouteNumber = 0;
            _isPositiveRouteChange = true;
            _target = routePoints[0];
        }
        else
        {
            _routePointsCount = 0;
        }

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
        if (transform.position.y!=2.5f)
        {
            transform.position = new Vector3(transform.position.x, 2.5f, transform.position.z);
        }

        if (_knockoutRemainder < 0)
        {
            
            if ( (Math.Abs(transform.eulerAngles.x-90))<0.5f)
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
            //Debug.Log("Enemy Tipe B _knockoutКemainder=" + _knockoutКemainder.ToString());
        }
        
    }
}*/

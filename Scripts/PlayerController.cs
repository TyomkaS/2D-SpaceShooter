using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public class PlayerController : MonoBehaviour, IDamagebale
{
    public Action<GameObject> OnDestroy;
    public Action<GameObject, int> OnDamage;

    [SerializeField] private float health;
    [SerializeField] private float movingFwdSpeed;
    [SerializeField] private float movingBackSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float reloadTime;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Transform bullet;
    [SerializeField] private Transform shootStartPosition;
    [SerializeField] private Transform dyingball;

    private Rigidbody _rb;
    private bool _hasRigidbody;
    private Quaternion _targetRotation;

    private float _reloadTime;
    private bool _isDeathActivated;
    private float _dieDelay;

    public void Die()
    {
        if (!_isDeathActivated)
        {
            Transform ballObj = Instantiate(dyingball, transform.position, Quaternion.identity);
            DamageBall ball = ballObj.GetComponent<DamageBall>();

            ball.followTo = transform;
            _dieDelay = (ball.maximumScale / ball.growthSpeed)*0.5f;
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
                OnDestroy?.Invoke(this.gameObject);
                Destroy(this.gameObject);
            }
        }
    }

    public float GetHealthF()
    {
        if (health > 0)
        {
            return health;
        }
        else
        {
            return 0;
        }
    }

    public int GetHealthI()
    {
        if (health > 0)
        {
            return (int)health;
        }
        else
        {
            return 0;
        }
    }
    private void MoveBack()
    {
        transform.Translate(Vector3.up * (-movingBackSpeed) * Time.deltaTime);
    }
    private void MoveFwd()
    {
        transform.Translate(Vector3.up * movingFwdSpeed * Time.deltaTime);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        

        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Debug.Log("Player Asteroid collision");
            TakeDamage(collision.impulse * 75f);
        }

        _rb.AddForce(new Vector3(collision.impulse.x, 0.0f, collision.impulse.z), ForceMode.Force);

    }

    public void TakeDamage(Vector3 damage)
    {
        float result = (float)Math.Sqrt(damage.x * damage.x + damage.y * damage.y);
        health -= result;
        TransmitHelath();
        Debug.Log("Player health = " + health.ToString());


    }

    public void TakeScalarDamage(float damage)
    {
        health -= damage;
        TransmitHelath();
        Debug.Log("Player health = " + health.ToString());
    }

    private void TransmitHelath()
    {
        int transmitted;
        if (health > 0)
        {
            transmitted = (int)health;
        }
        else
        {
            transmitted = 0;
        }
        OnDamage?.Invoke(this.gameObject, transmitted);
    }

    private void Turn—lockwise()
    {
        transform.Rotate(0.0f, turnSpeed * Time.deltaTime, 0.0f, Space.World);
    }

    private void Turn—ounter—lockwise()
    {
        transform.Rotate(0.0f, -turnSpeed * Time.deltaTime, 0.0f, Space.World);
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
            //transform.TransformDirection(Vector3.up) - ÔÓÎÛ˜ÂÌËÂ Ì‡Ô‡‚ÎÂÌËˇ Ë„ÓÍ‡, ˜ÚÓ·˚ ÔÛÎË ÒÚÂÎˇÎË ÔˇÏÓ ÔÂÂ‰ Ë„ÓÍÓÏ
            //Debug.Log("Shoot transform fwd" + transform.TransformDirection(Vector3.up));
            pb.SetVelocity(transform.TransformDirection(Vector3.up * bulletSpeed));

            _reloadTime = reloadTime;
        }

    }

    private void FixedUpdate()
    {
        //›ÚÓ ÏÓÊÌÓ ·Û‰ÂÚ Û‰‡ÎËÚ¸, Ú.Í. ÌÛÊÌÓ ÚÓÎ¸ÍÓ ‰Îˇ ÓÚÎ‡‰ÍË
        Ray fwd, up, w;

        fwd = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        up = new Ray(transform.position, transform.TransformDirection(Vector3.up));
        w = new Ray(transform.position, transform.InverseTransformDirection(Vector3.up));

        Debug.DrawRay(fwd.origin, fwd.direction * 8, Color.blue);
        Debug.DrawRay(up.origin, up.direction * 8, Color.red);
        Debug.DrawRay(w.origin, w.direction * 8, Color.green);
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
        _reloadTime = 0;
        _isDeathActivated = false;

        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _hasRigidbody = true;
            _rb.constraints = RigidbodyConstraints.FreezePositionY;                             //Ì‡ÎÓÊÂÌËÂ ÔÂÂÏÂ˘ÂÌËˇ ‚‡˘ÂÌËˇ ÔÓ ÓÒË Y, ˜ÚÓ·˚ ÌÂ ÒÏÂ˘‡ÎÒˇ ÔÓ ‚ÂÏˇ ÒÚÓÎÍÌÓ‚ÂÌËÈ
            _rb.constraints = RigidbodyConstraints.FreezeRotationZ;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX;
            //_rb.freezeRotation = true;

        }

        _targetRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        //Debug.Log("Player rotation" + transform.rotation);

    }


    // Update is called once per frame
    void Update()
    {
        if (health > 0)
        {
            if (transform.position.y != 2.5f)
            {
                transform.position = new Vector3(transform.position.x, 2.5f, transform.position.z);
            }

            if ((Math.Abs(transform.eulerAngles.x - 90)) < 0.5f)
            {
                //if (Input.GetKey(KeyCode.W))
                //{
                //    MoveFwd();
                //}
                //if (Input.GetKey(KeyCode.S))
                //{
                //    MoveBack();
                //}
                //if (Input.GetKey(KeyCode.D))
                //{
                //    Turn—lockwise();
                //}
                //if (Input.GetKey(KeyCode.A))
                //{
                //    Turn—ounter—lockwise();
                //}

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    MoveFwd();
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    MoveBack();
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    Turn—lockwise();
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    Turn—ounter—lockwise();
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    Shoot();
                }
            }
            else
            {
                StabilizeXRotation();
            }
        }
        else
        {
            Die();
        }
        

        

        //Debug.Log("Reload time =" + _reloadTime.ToString());
        if (_reloadTime > 0)
        {
            _reloadTime -= Time.deltaTime;
        }
    }
}

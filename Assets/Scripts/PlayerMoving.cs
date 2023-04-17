using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public static class PlayerMovingAnimator
{
    public static class Params
    {
        public const string MoveX = "MoveX";
        public const string AttackTrigger = "AttackTrigger";
        public const string IsOnGround = "IsOnGround";
        public const string AirSpeedY = "AirSpeedY";
        public const string RollTrigger = "RollTrigger";
        public const string IsOnWall = "IsOnWall";

        public static class States
        {
            public const string Idle = nameof(Idle);
            public const string Run = nameof(Run);
            public const string Attack1 = nameof(Attack1);
            public const string Jump = nameof(Jump);
            public const string Fall = nameof(Fall);
            public const string Roll = nameof(Roll);
        }
    }
}

public class PlayerMoving : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _jumpForce = 25f;
    [SerializeField] private Transform _groundChecker;

    [SerializeField] private Transform _wallCheckerUp;
    [SerializeField] private Transform _wallCheckerDown;

    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wall;

    [SerializeField] private float _slideSpeed = 4f;

    private Vector2 _moveVector;
    private Rigidbody2D _rigidbody;
    private Animator _animator;

    private float _gravityDefault;
    private float _groundCheckRadius;
    private float _wallCheckRadius;

    private bool _isfaceRight = true;
    private bool _isOnGround;
    private bool _isOnWall;


    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _groundCheckRadius = _groundChecker.GetComponent<CircleCollider2D>().radius;
        _wallCheckRadius = _wallCheckerUp.GetComponent<CircleCollider2D>().radius;
        _gravityDefault = _rigidbody.gravityScale;
    }
    
    private void Update()
    {
        if (!_isRolling && !_isWallJumping)
        {
        Walk();
        }

        Reflect();

        if (Input.GetKeyDown(KeyCode.Space) && _isOnGround == true)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _isCanRoll && _isOnGround)
        {
            StartCoroutine(Roll());
        }

        WallJump();
        CheckGround();
        CheckWall();
        MoveOnWall();
    }

    private void Walk()
    {
        _moveVector.x = Input.GetAxis("Horizontal");
        _animator.SetFloat(PlayerMovingAnimator.Params.MoveX, Mathf.Abs(_moveVector.x));
        _rigidbody.velocity = new Vector2(_moveVector.x * _speed, _rigidbody.velocity.y);
    }

    private void Reflect()
    {
        if ((_moveVector.x > 0 && !_isfaceRight || (_moveVector.x < 0 && _isfaceRight)))
        {
            transform.localScale *= new Vector2(-1, 1);
            _isfaceRight = !_isfaceRight;
        }
    }

    private void Jump()
    {     
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpForce);

    }

    private void CheckGround()
    {
        _isOnGround = Physics2D.OverlapCircle(_groundChecker.position, _groundCheckRadius, _groundLayer);
        _animator.SetBool(PlayerMovingAnimator.Params.IsOnGround, _isOnGround);
        _animator.SetFloat(PlayerMovingAnimator.Params.AirSpeedY, _rigidbody.velocity.y);

    }

    private bool _isCanRoll = true;
    private bool _isRolling;
    private float _rollPower = 15;
    private float _rollingTime = 0.5f;
    private float _rollCooldown = 0.5f;


    private void CheckWall()
    {
        _isOnWall = Physics2D.OverlapCircle(_wallCheckerUp.position, _wallCheckRadius, _wall) && Physics2D.OverlapCircle(_wallCheckerDown.position, _wallCheckRadius, _wall);
    }

    private void MoveOnWall()
    {

        if (_isOnWall && !_isOnGround && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            _rigidbody.gravityScale = 0;
            _rigidbody.velocity = new Vector2(0, -_slideSpeed);
            _animator.SetBool(PlayerMovingAnimator.Params.IsOnWall, true);
        }


        else if (!_isOnWall || _isOnGround)
        {
            _rigidbody.gravityScale = _gravityDefault;
            _animator.SetBool(PlayerMovingAnimator.Params.IsOnWall, false);

        }

    }


    private IEnumerator Roll()
    {
        _isCanRoll = false;
        _isRolling = true;
        _rigidbody.velocity = new Vector2(0, 0);
        _animator.SetTrigger(PlayerMovingAnimator.Params.RollTrigger);

        if (_isfaceRight)
        {
            _rigidbody.AddForce(new Vector2(_rollPower * 1, 0f), ForceMode2D.Impulse);
        }
        else
        {
            _rigidbody.AddForce(new Vector2(_rollPower * (-1), 0f), ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(_rollingTime);
        _isRolling = false;
        yield return new WaitForSeconds(_rollCooldown);
        _isCanRoll = true;
    }

    private bool _isWallJumping = false;
    private float _timerWallJump;
    private float _wallJumpTime = 0.5f;
    private Vector2 _jumpAngle = new Vector2(1, 1);

    private void WallJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _isOnWall && _isOnGround == false)
        {
            _isWallJumping = true;
            _moveVector.x = 0;
            transform.localScale *= new Vector2(-1, 1);
            _isfaceRight = !_isfaceRight;
            _rigidbody.velocity = new Vector2(transform.localScale.x * _jumpAngle.x, _jumpAngle.y);
            _rigidbody.gravityScale = _gravityDefault;
            _rigidbody.velocity = new Vector2(0, 0);

            if ((_timerWallJump += Time.deltaTime) >= _wallJumpTime)
            {
                if (_isOnWall || _isOnGround || Input.GetAxis("Horizontal") != 0)
                {
                    _isWallJumping = false;
                    _timerWallJump = 0;
                }
            }
        }
    }
}

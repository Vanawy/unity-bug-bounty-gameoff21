using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    private float _inputY = 0, _inputX = 0;
    private bool _inputJump, _inputJumpDown;

    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _sr;

    [Header("Jump")]
    [SerializeField]
    [Range(1, 15)]
    private float _jumpVelocity = 5f;
    [SerializeField]
    [Range(0, 5)]
    private float _fallMultiplier = 2.5f;
    [SerializeField]
    [Range(0, 5)]
    private float _lowJumpMultiplier = 2f;
    [SerializeField]
    private float _wallSlideMultiplier = 0.5f;
    [SerializeField]
    [Tooltip("Time when player cant change direction after walljump")]
    private float _wallJumpMoveDelay = 0.2f;
    private float _lastWallJumpTime = -1;
    [SerializeField]
    private float _jumpDelay = 0.1f;
    private float _lastJumpTime = -1; 
    [SerializeField]
    [Tooltip("0 - is vertical, 1 is horizontal")]
    [Range(0,1)]
    private float _wallJumpDirection = 0.3f;

    [SerializeField]
    private ActiveTrigger _floorTrigger;
    [SerializeField]
    private ActiveTrigger _leftTrigger;
    [SerializeField]
    private ActiveTrigger _rightTrigger;

    [Header("Move")]
    [SerializeField]
    private float _maxSpeed = 2f;
    private bool _isJumping = false; 
    private bool _isOnWall, _isLeftWall = false;


    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        _inputX = Input.GetAxis("Horizontal");
        _inputY = Input.GetAxis("Vertical");
        _inputJumpDown = Input.GetButtonDown("Jump") || _inputJumpDown;
        _inputJump = Input.GetButton("Jump");
    }

    void FixedUpdate() {
        _isJumping = !_floorTrigger.IsActive();
        _isLeftWall = _leftTrigger.IsActive();
        _isOnWall = (_rightTrigger.IsActive() || _leftTrigger.IsActive()) 
                    && (_isLeftWall ? _inputX < 0 : _inputX > 0)
                    && _isJumping;

        if (CanJump()) {
            Jump();
        }
        BetterJump();
        OnWall();
        Move();
        _inputJumpDown = false;
        UpdateAnimator();
    }

    void LateUpdate() {
        UpdateSprite();
    }

    private bool CanJump()
    {
        if (_isJumping) return false;
        if (_inputJump) return true;
        return false;
    }

    private void Jump()
    {
        if (!CanJumpAgain()) return;
        _rb.AddForce(Vector2.up * _jumpVelocity, ForceMode2D.Impulse);
    }

    private void BetterJump()
    {
        if(_isOnWall) return;
        if (_rb.velocity.y < 0) {
			_rb.AddForce(Physics.gravity * (_fallMultiplier - 1));
            Debug.DrawRay(transform.position, Physics.gravity * (_fallMultiplier - 1), Color.red);
        } else if (_rb.velocity.y > 0 && _inputJump) {
			_rb.AddForce(Physics.gravity * (_lowJumpMultiplier - 1));
            Debug.DrawRay(transform.position, Physics.gravity * (_lowJumpMultiplier - 1), Color.green);
        }
    }

    private void OnWall()
    {
        if (!_isOnWall) return;
        if (_rb.velocity.y < 0) {
			_rb.AddForce(Vector2.up * Physics.gravity.y * (_wallSlideMultiplier - 1));
        }
        if (_inputJump) {
            _lastWallJumpTime = Time.time;
            Vector2 jumpDirection = Vector2.Lerp(Vector2.up, (_isLeftWall ? Vector2.right : Vector2.left), _wallJumpDirection);
            Debug.DrawRay(_rb.position, jumpDirection);
            _rb.velocity = jumpDirection.normalized * _jumpVelocity;
        }
    }

    private void UpdateSprite()
    {
        if (!CanMoveAfterWalljump()) return;
        if (_inputX < 0) { 
            _sr.flipX = true && !_isOnWall;
        } else if (_inputX > 0) { 
            _sr.flipX = false || _isOnWall;
        }
    }

    private void Move()
    {
        if (!CanMoveAfterWalljump()) return;
        _rb.velocity = new Vector2(_inputX * _maxSpeed, _rb.velocity.y);
    }

    private bool CanMoveAfterWalljump()
    {
        return Time.time - _lastWallJumpTime > _wallJumpMoveDelay;
    }

    private bool CanJumpAgain()
    {
        return Time.time - _lastJumpTime > _jumpDelay;
    }

    private void UpdateAnimator()
    {
        _animator.SetBool("is_jumping", _isJumping);
        _animator.SetFloat("speed", _rb.velocity.magnitude);
        _animator.SetBool("is_on_wall", _isOnWall);
    }
}
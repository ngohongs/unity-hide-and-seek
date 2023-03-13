using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameManager gameManager;
    public UIController uiController;
    public AudioSource audioSource;
    public AudioClip rollSound;
    public AudioClip jumpSound;
    public FixedJoystick joystick;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _maxSpeed = 10;
    [SerializeField] private float _minSpeed = 5;
    [SerializeField] private float _turnSpeed = 360;

    public Vector3 _input = Vector3.zero;
    private bool _jump = false;
    private bool _roll = false;

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
            _input = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
        Look();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void Reset()
    {
        foreach (var param in _animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                _animator.ResetTrigger(param.name);
            }
        }
        _animator.Play("Looking Around");
    }     

    private void Look()
    {
        if (_input == Vector3.zero) return;
        var rot = Quaternion.LookRotation(_input.ToIso(), Vector3.up);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rot, _turnSpeed * Time.deltaTime);
    }

    private void OnMove(InputValue value)
    {
        if (Application.platform == RuntimePlatform.Android)
            return;
        var val = value.Get<Vector2>();
        _input = Vector3.zero;
        _input.x = val.x;
        _input.z = val.y;
    }

    private void OnJump()
    {
        _jump = true;     
    }

    private void OnRoll()
    {
        _roll = true;
    }

    private void OnEscape()
    {
        if (gameManager.isEnd)
        {
            gameManager.UserPauseOnOff();
            uiController.PlayButtonClicked();
        }

        if (!gameManager.isPaused)
        {
            uiController.Show(UIController.UIType.GameMenu);
            gameManager.UserPauseOnOff();
        }
        else
        {
            uiController.HideUI();
            gameManager.UserPauseOnOff();
        }
    }

    private void Move()
    {
        if (_roll && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Grab Jump"))
        {
            audioSource.PlayOneShot(rollSound);
            _animator.SetTrigger("Grab Jump");
            _roll = false;
        }

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Grab Jump"))
        {
            return;
        }

        if (_jump && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            audioSource.PlayOneShot(jumpSound);
            _animator.SetTrigger("Jump");
            _jump = false;

        }

        float speed = Mathf.Lerp(_minSpeed, _maxSpeed, _input.magnitude);
        Vector3 moveVector = speed * _input.ToIso().normalized;
        _rigidbody.velocity += moveVector;
        _animator.SetFloat("Velocity", _input.magnitude);
    }

  
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}

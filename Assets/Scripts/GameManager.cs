using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NeonReflexVR
{
    public enum GameState { Ready, Playing, GameOver }

    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] TargetSpawner spawner;
        [SerializeField] InputActionReference restartAction;
        [SerializeField] Text statusText;
        float deadline;
        int displayedScore = -1;
        int displayedSeconds = -1;
        GameState displayedState = (GameState)(-1);

        public GameState State { get; private set; } = GameState.Ready;
        public int Score { get; private set; }
        public float RemainingSeconds { get; private set; } = 60f;
        public InputActionReference RestartAction => restartAction;
        public TargetSpawner Spawner => spawner;
        public Text StatusText => statusText;

        void LateUpdate()
        {
            if (State == GameState.Ready) StartGame(Time.time);
            Tick(Time.time);
            if (State == GameState.GameOver && restartAction != null && restartAction.action != null && restartAction.action.WasPressedThisFrame())
                Restart(Time.time);
        }

        public bool StartGame(float now)
        {
            if (State != GameState.Ready) return false;
            BeginRound(now);
            return true;
        }

        public bool Restart(float now)
        {
            if (State != GameState.GameOver) return false;
            BeginRound(now);
            return true;
        }

        void BeginRound(float now)
        {
            Score = 0;
            RemainingSeconds = 60f;
            deadline = now + 60f;
            State = GameState.Playing;
            if (spawner != null)
            {
                spawner.Stop();
                var view = spawner.View;
                if (view != null)
                {
                    spawner.Initialize(view.position, view.forward, now);
                    if (statusText != null)
                    {
                        var forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
                        if (forward.sqrMagnitude < 0.5f) forward = Vector3.forward;
                        var canvas = statusText.canvas.transform;
                        canvas.position = view.position + forward * 2f + Vector3.up * 0.55f;
                        canvas.rotation = Quaternion.LookRotation(forward, Vector3.up);
                    }
                }
            }
            RefreshDisplay();
        }

        public void Tick(float now)
        {
            if (State != GameState.Playing) return;
            RemainingSeconds = Mathf.Max(0f, deadline - now);
            if (RemainingSeconds <= 0f)
            {
                State = GameState.GameOver;
                if (spawner != null) spawner.Stop();
            }
            else if (spawner != null) spawner.Tick(now);
            RefreshDisplay();
        }

        public bool TryHit(Target target, float now)
        {
            // Check the deadline even if this controller runs before our LateUpdate.
            Tick(now);
            if (State != GameState.Playing || target == null || !target.TryHit(now)) return false;
            Score++;
            RefreshDisplay();
            return true;
        }

        void RefreshDisplay()
        {
            var seconds = Mathf.CeilToInt(RemainingSeconds);
            if (statusText == null || (displayedScore == Score && displayedSeconds == seconds && displayedState == State)) return;
            displayedScore = Score;
            displayedSeconds = seconds;
            displayedState = State;
            statusText.text = State == GameState.GameOver
                ? "Game Over\nFinal Score: " + Score + "\nTime: 0\nPress A to Restart"
                : "Score: " + Score + "\nTime: " + seconds;
        }

        void OnDisable()
        {
            if (spawner != null) spawner.Stop();
            State = GameState.Ready;
        }
    }
}
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
        [SerializeField] Image backdrop;
        Vector3 uiAnchor;
        Vector3 uiDirection = Vector3.forward;
        bool menuAnchored;
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

        void OnEnable() => RefreshDisplay();

        void LateUpdate()
        {
            if (State == GameState.Ready && !menuAnchored && spawner != null &&
                spawner.TryGetStartPose(out uiAnchor, out uiDirection))
            {
                menuAnchored = true;
                PlaceUI();
            }
            RefreshDisplay();
            Tick(Time.time);
            if (restartAction == null || restartAction.action == null || !restartAction.action.WasPressedThisFrame()) return;
            if (State == GameState.Ready) StartGame(Time.time);
            else if (State == GameState.GameOver) Restart(Time.time);
        }
        public bool StartGame(float now)
        {
            if (State != GameState.Ready) return false;
            return BeginRound(now);
        }

        public bool Restart(float now)
        {
            if (State != GameState.GameOver) return false;
            return BeginRound(now);
        }

        bool BeginRound(float now)
        {
            var position = Vector3.zero;
            var direction = Vector3.forward;
            // Keep Ready/GameOver and the final score intact until a valid HMD pose is available.
            if (spawner != null && !spawner.TryGetStartPose(out position, out direction)) return false;
            Score = 0;
            RemainingSeconds = 60f;
            deadline = now + 60f;
            State = GameState.Playing;
            uiAnchor = position;
            uiDirection = direction;
            if (spawner != null)
            {
                spawner.Stop();
                spawner.Initialize(position, direction, now);
            }
            RefreshDisplay();
            return true;
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

        void PlaceUI()
        {
            if (statusText == null) return;
            var rect = (RectTransform)statusText.canvas.transform;
            var playing = State == GameState.Playing;
            rect.sizeDelta = new Vector2(900f, playing ? 180f : 440f);
            if (State != GameState.Ready || menuAnchored)
            {
                rect.position = uiAnchor + uiDirection * 2f + Vector3.up * (playing ? 0.6f : 0.05f);
                rect.rotation = Quaternion.LookRotation(uiDirection, Vector3.up);
            }
            if (backdrop != null) backdrop.color = new Color(0.02f, 0.035f, 0.055f, playing ? 0.65f : 0.88f);
        }

        void RefreshDisplay()
        {
            var seconds = Mathf.CeilToInt(RemainingSeconds);
            if (statusText == null || (displayedScore == Score && displayedSeconds == seconds && displayedState == State)) return;
            if (displayedState != State) PlaceUI();
            displayedScore = Score;
            displayedSeconds = seconds;
            displayedState = State;
            if (State == GameState.Ready)
                statusText.text = "<size=64><color=#67DFFC>NEON REFLEX VR</color></size>\n\n<size=30>Shoot the targets.\nScore as many points as possible in 60 seconds.</size>\n\n<size=44><color=#FFAD66>Press A to Start</color></size>";
            else if (State == GameState.GameOver)
                statusText.text = "<size=64><color=#67DFFC>GAME OVER</color></size>\n\n<size=48>FINAL SCORE: <color=#FFAD66>" + Score + "</color></size>\n\n<size=44>Press A to Play Again</size>";
            else
                statusText.text = "<size=64><color=#67DFFC>SCORE:</color> " + Score + "\n<color=#FFAD66>TIME:</color> " + seconds + "</size>";
        }
        void OnDisable()
        {
            if (spawner != null) spawner.Stop();
            State = GameState.Ready;
            menuAnchored = false;
            displayedState = (GameState)(-1);
        }
    }
}
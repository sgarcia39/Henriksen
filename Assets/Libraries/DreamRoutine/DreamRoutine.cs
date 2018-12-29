using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Dreamtastic
{
    //! Routine Type
    /*!
        Type assigned to a Routine
    */
    public enum DRType
    {
        None,
        Update,
        TimedUpdate,
        LateUpdate,
#if DR_FIXEDUPDATE
        FixedUpdate,
#endif
    }

    //! Routine Id
    /*!
        Unique id assigned to a Routine. A new DRId is created per use of DreamRoutine.StartRoutine.
        \n\n
        See also: DreamRoutine.PauseRoutine(DRId), DreamRoutine.StopRoutine(DRId)
    */
    public struct DRId
    {
        private static System.Random _qualifierGenerator = new System.Random();
        private int _index;
        private int _qualifier;

        /*!
            Index assigned by DreamRoutine
        */
        public int index { get { return _index; } }

        /*!
            Random id generated per DRId
        */
        public int qualifier { get { return _qualifier; } }

        /*!
            Creates a routine id.
            \warning Not intended to be used directly. DreamRoutine assumes responsibilty for assuring all DRId's are created correctly.
        */
        public DRId(int idIndex)
        {
            _index = idIndex;
            _qualifier = _qualifierGenerator.Next(1, int.MaxValue);
        }

        /*!
            Compares two objects to see if they are the same class and value.
        */
        public override bool Equals(System.Object systemObject)
        {
            return systemObject is DRId && this == (DRId)systemObject;
        }

        public override int GetHashCode()
        {
            return _index.GetHashCode() ^ _qualifier.GetHashCode();
        }

        /*!
            Compares two objects to see if they are the same value.
        */
        public static bool operator ==(DRId x, DRId y)
        {
            return x.index == y.index && x.qualifier == y.qualifier;
        }

        /*!
            Compares two objects to see if they are different values.
        */
        public static bool operator !=(DRId x, DRId y)
        {
            return !(x == y);
        }
    }

    //! Base Routine Instruction
    /*!
        Base class for all DreamRoutine instructions. Not intended to be used directly.
    */
    public interface IDRInstruction
    {
        DRType type { get; }
        float time { get; }
    }

    //! Update Instruction
    /*!
        Update instruction setup is similar to that of a null YieldInstruction for Coroutines.
        \n\n
        Unique to DRUpdate, null instructions can also be used to acheive the same purpose.
        Both null and DRUpdate instructions are handled identical to one another.
        \n\n
        \b Note: Routine logic is processed in Monobehavior.Update.
        \code{.cs}
        private IEnumerator<IDRInstruction> Routine()
        {
            while(true)
            {
                yield return DreamRoutine.update;
            }
        }
        \endcode
        \n
        \code{.cs}
        private IEnumerator<IDRInstruction> Routine()
        {
            while(true)
            {
                yield return null;
            }
        }
        \endcode
    */
    public class DRUpdate : IDRInstruction
    {
        private DRType _type;

        public DRType type { get { return _type; } }
        public float time { get { return 0; } }

        /*!
            Creates an instruction that executes on Monobehavior.Update.
        */
        public DRUpdate()
        {
            _type = DRType.Update;
        }
    }

    //! Timed Routine Instruction
    /*!
        Timed instruction setup is similar to that of a WaitForSeconds YieldInstruction for Coroutines.
        \n\n
        Suspends the routine execution for the given amount of seconds.
        \n\n
        \b Note: Routine logic is processed in Monobehavior.Update.
        \code{.cs}
        private DRTimedUpdate timedUpdate = new DRTimedUpdate(5);

        private IEnumerator<IDRInstruction> Routine()
        {
            while(true)
            {
                yield return timedUpdate;
            }
        }
        \endcode
    */
    public class DRTimedUpdate : IDRInstruction
    {
        private DRType _type;
        private float _time;

        public DRType type { get { return _type; } }
        public float time { get { return _time; } }

        /*!
            Creates an instruction that executes on Monobehavior.Update. Waits for a given number of seconds before execution.
            \param instructionTime Number of seconds for instruction to wait.
        */
        public DRTimedUpdate(float instructionTime)
        {
            _type = DRType.TimedUpdate;
            _time = instructionTime;
        }
    }

    //! Late Update Routine Instruction
    /*!
        Update instruction setup is similar to that of a null YieldInstruction for Coroutines.
        \n\n
        \b Note: Routine logic is processed in Monobehavior.LateUpdate.
        \code{.cs}
        private IEnumerator<IDRInstruction> Routine()
        {
            while(true)
            {
                yield return DreamRoutine.lateUpdate;
            }
        }
        \endcode
    */
    public class DRLateUpdate : IDRInstruction
    {
        private DRType _type;

        public DRType type { get { return _type; } }
        public float time { get { return 0; } }

        /*!
            Creates an instruction that executes on Monobehavior.LateUpdate
        */
        public DRLateUpdate()
        {
            _type = DRType.LateUpdate;
        }
    }

#if DR_FIXEDUPDATE
    //! Late Update Routine Instruction
    /*!
        Update instruction setup is similar to that of a null YieldInstruction for Coroutines.
        For performance reasons, ony use if required by game.
        \n\n
        \b Note: Routine logic is processed in Monobehavior.FixedUpdate.
        \n\n
        \b Note: The platform custom define \b DR_FIXEDUPDATE must be set prior to use.
        For more information on platform defines, see https://docs.unity3d.com/Manual/PlatformDependentCompilation.html</b>
        \code{.cs}
        private IEnumerator<IDRInstruction> Routine()
        {
            while(true)
            {
                yield return DreamRoutine.fixedUpdate;
            }
        }
        \endcode
    */
    public class DRFixedUpdate : IDRInstruction
    {
        private DRType _type;

        public DRType type { get { return _type; } }
        public float time { get { return 0; } }

        /*!
            Creates an instruction that executes on Monobehavior.FixedUpdate
        */
        public DRFixedUpdate()
        {
            _type = DRType.FixedUpdate;
        }
    }
#endif

    //! Dream Routine Manager
    /*!
        Performant routine manager. Routines created by DreamRoutine incur virtually no performance overhead.

        Routine management occurs in the scripted langauge and executed through one GameObject instance.
        This allows DreamRoutine to avoid incurring extra performance penalties by not having to traverse from unmanaged to
        managed code per Coroutine GameObject.

        Routines can be instructed to run through GameObject's Update, LateUpdate, or FixedUpdate pipelines.

        Realtime debug information can be viewed from the GameObject \b ~DreamRoutine in the UnityEditor inspector.

        \b Note: Routines executed do not trigger additional GC allocations per frame.
    */
    public class DreamRoutine : MonoBehaviour
    {
        private class DRRoutine
        {
            public int index;
            public bool active;
            public bool paused;
            public float deltaTime;
            public DRId id;
            public DRType type;
            public GameObject gameObject;
            public IEnumerator<IDRInstruction> instruction;

            public DRRoutine(int dataIndex)
            {
                index = dataIndex;
            }

            public void Start(GameObject dataGameObject, IEnumerator<IDRInstruction> dataInstruction)
            {
                id = new DRId(index);
                type = DRType.None;
                gameObject = dataGameObject;
                instruction = dataInstruction;
                paused = false;
                deltaTime = 0;

                active = true;
            }

            public void Stop()
            {
                active = false;

                type = DRType.None;
                gameObject = null;
                instruction = null;
                paused = false;
                deltaTime = 0;
            }
        }

        public static IDRInstruction update = new DRUpdate();
        public static IDRInstruction lateUpdate = new DRLateUpdate();
#if DR_FIXEDUPDATE
        public static IDRInstruction fixedUpdate = new DRFixedUpdate();
#endif
        private static DRRoutine[] _routines;
        private static int _routineMaxIndex = -1;
        private static int _routineMax = 10000;
        private static GameObject _globalObject;

        /*!
            Specifies if DreamRoutine is initialized
        */
        public static bool isInit { get { return _routines != null; } }

        public static int activeRoutineCount { get { return _routineMaxIndex + 1; } }

        public static int maxRoutineCount { get { return _routineMax; } }

        /*!
            Pre-initialized DRUpdate instruction. Instruction instance can be used simultaneously in multiple routines.
            \note It is advisable to use this cached instance.
        */
        // public static IDRInstruction update { get { return _update; } }
        /*!
            Pre-initialized DRLateUpdate instruction. Instruction instance can be used simultaneously in multiple routines.
            \note It is advisable to use this cached instance.
        */
        // public static IDRInstruction lateUpdate { get { return _lateUpdate; } }
#if DR_FIXEDUPDATE
        /*!
            Pre-initialized DRFixedUpdate instruction. Instruction instance can be used simultaneously in multiple routines.
            \note It is advisable to use this cached instance.
        */
        // public static IDRInstruction fixedUpdate { get { return _fixedUpdate; } }
#endif

        /*!
            Initializes DreamRoutine. If DreamRoutine is already initilized, does nothing.
            \note DreamRoutine does not have to be explicitly initialized. All static methods either check if
            initialized or initialize if required.
        */
        public static void Init()
        {
            if (!isInit)
            {
                _globalObject = new GameObject();
                _globalObject.name = "~DreamRoutine";
                _globalObject.isStatic = true;
                _globalObject.AddComponent<DreamRoutine>();
#if !UNITY_EDITOR
                _globalObject.hideFlags = HideFlags.HideAndDontSave;
#endif
                DontDestroyOnLoad(_globalObject);

                _routines = new DRRoutine[_routineMax];
                for(int idx = 0; idx < _routineMax; ++idx)
                {
                    _routines[idx] = new DRRoutine(idx);
                }
            }
        }

        public static void Init(int routineMax)
        {
            if (!isInit)
            {
                _routineMax = routineMax;
                Init();
            }
        }

        /*!
            Resets DreamRoutine and destroys it's GameObject instance. All active and paused routines will stop executing.
        */
        public static void Reset()
        {
            if (isInit)
            {
                for(int idx = 0; idx < _routineMax; ++idx)
                {
                    _routines[idx].active = false;
                }
                _routines = null;
                _routineMax = 10000;
                _routineMaxIndex = -1;
                Destroy(_globalObject);
            }
        }

        /*!
            Starts a routine.

            Similar to Monobehavior.StartCoroutine, the execution of the routine can be paused by yield statement
            within the instruction method. A GameObject instance is used to help manage the lifespan
            of the routine. If the GameObject ceases to exist, the routine will stop executing.
            \param routineGameObject GameObject representing a Routine.
            \param routineInstruction IEnumerator routine instruction method.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                private void Start()
                {
                    DreamRoutine.StartRoutine(gameObject, Routine());
                }

                private IEnumerator<IDRInstruction> Routine()
                {
                    while(true)
                    {
                        yield return null;
                    }
                }
            }
            \endcode
        */
        public static DRId StartRoutine(IEnumerator<IDRInstruction> routineInstruction)
        {
            Init();
            DRRoutine routine = null;
            for (int idx = 0; idx < _routineMax; ++idx)
            {
                if (_routines[idx].active == false)
                {
                    routine = _routines[idx];
                    break;
                }
            }

            if (routine != null)
            {
                StartRoutine(routine, _globalObject, routineInstruction);
                return routine.id;
            }
            else
            {
                Debug.LogError("RoutineSystem.StartRoutine : Number of simultaneous routines exceeded max value : " + _routineMax);
                return new DRId();
            }
        }

        public static DRId StartRoutine(GameObject routineGameObject,
            IEnumerator<IDRInstruction> routineInstruction)
        {
            Init();
            DRRoutine routine = null;
            for (int idx = 0; idx < _routineMax; ++idx)
            {
                if (_routines[idx].active == false)
                {
                    routine = _routines[idx];
                    break;
                }
            }

            if (routine != null)
            {
                StartRoutine(routine, routineGameObject, routineInstruction);
                return routine.id;
            }
            else
            {
                Debug.LogError("RoutineSystem.StartRoutine : Number of simultaneous routines exceeded max value : " + _routineMax);
                return new DRId();
            }
        }

        /*!
            Pauses the execution of a specific routine via an id.
            \param routineId Unique id representing a routine.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                DRId routineId;

                private void Start()
                {
                    DRId routineId = DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnPauseRoutine()
                {
                    DreamRoutine.PauseRoutine(routineId);
                }
            }
            \endcode
        */
        public static void PauseRoutine(DRId routineId)
        {
            if (isInit)
            {
                DRRoutine routine = _routines[routineId.index];
                if (routine.id == routineId)
                {
                    routine.paused = true;
                }
            }
        }

        /*!
            Pauses the execution of all routines represented a GameObject.
            \param routineId Unique id representing a routine.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                private void Start()
                {
                    DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnPauseRoutine()
                {
                    DreamRoutine.PauseRoutines(gameObject);
                }
            }
            \endcode
        */
        public static void PauseRoutines(GameObject routineGameObject)
        {
            if (isInit)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    DRRoutine routine = _routines[idx];
                    if (routine.gameObject == routineGameObject)
                    {
                        routine.paused = true;
                    }
                }
            }
        }

        /*!
            Pauses the execution of all routines.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                private void Start()
                {
                    DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnPauseRoutines()
                {
                    DreamRoutine.PauseRoutines();
                }
            }
            \endcode
        */
        public static void PauseRoutines()
        {
            if (isInit)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    _routines[idx].paused = true;
                }
            }
        }

        /*!
            Resumes the execution of a specific routine via an id.
            \param routineId Unique Id representing a Routine.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                DRId routineId;

                private void Start()
                {
                    DRId routineId = DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnResumeRoutine()

                    DreamRoutine.ResumeRoutine(routineId);
                }
            }
            \endcode
        */
        public static void ResumeRoutine(DRId routineId)
        {
            if (isInit)
            {
                DRRoutine routine = _routines[routineId.index];
                if (routine.id == routineId)
                {
                    routine.paused = false;
                }
            }
        }

        /*!
            Resumes the execution of all routines represented a GameObject.
            \param routineGameObject GameObject representing a Routine.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                private void Start()
                {
                    DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnResumeRoutine()
                {
                    DreamRoutine.ResumeRoutines(gameObject);
                }
            }
            \endcode
        */
        public static void ResumeRoutines(GameObject routineGameObject)
        {
            if (isInit)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    DRRoutine routine = _routines[idx];
                    if (routine.gameObject == routineGameObject)
                    {
                        routine.paused = false;
                    }
                }
            }
        }

        /*!
            Resumes the execution of all routines.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                private void Start()
                {
                    DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnResumeRoutines()
                {
                    DreamRoutine.ResumeRoutines();
                }
            }
            \endcode
        */
        public static void ResumeRoutines()
        {
            if (isInit)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    _routines[idx].paused = false;
                }
            }
        }

        /*!
            Stops the execution of a specific routine via an id.
            \param routineId Unique Id representing a Routine.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                DRId routineId;

                private void Start()
                {
                    DRId routineId = DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnStopRoutine()

                    DreamRoutine.StopRoutine(routineId);
                }
            }
            \endcode
        */
        public static void StopRoutine(DRId routineId)
        {
            if (isInit)
            {
                DRRoutine routine = _routines[routineId.index];
                if (routine.id == routineId)
                {
                    StopRoutine(routine);
                }
            }
        }

        /*!
            Stops the execution of all routines represented a GameObject.
            \param routineGameObject GameObject representing a Routine.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                private void Start()
                {
                    DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnStopRoutines()
                {
                    DreamRoutine.StopRoutines(gameObject);
                }
            }
            \endcode
        */
        public static void StopRoutines(GameObject routineGameObject)
        {
            if (isInit)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    DRRoutine routine = _routines[idx];
                    if (routine.gameObject == routineGameObject)
                    {
                        StopRoutine(routine);
                    }
                }
            }
        }

        /*!
            Stops the execution of all routines.

            \code{.cs}
            public class DreamRoutineBehavior : MonoBehaviour
            {
                private void Start()
                {
                    DreamRoutine.StartRoutine(gameObject, Routine());
                }

                ...

                public void OnStopRoutines()
                {
                    DreamRoutine.StopRoutines();
                }
            }
            \endcode
        */
        public static void StopRoutines()
        {
            if (isInit)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    StopRoutine(_routines[idx]);
                }
            }
        }

        public static bool IsRoutineActive(DRId routineId)
        {
            bool isActive = false;
            if (isInit)
            {
                DRRoutine routine = _routines[routineId.index];
                if (routine.id == routineId)
                {
                    isActive = routine.active;
                }
            }

            return isActive;
        }

        public static bool IsRoutinePaused(DRId routineId)
        {
            bool isPaused = false;
            if (isInit)
            {
                DRRoutine routine = _routines[routineId.index];
                if (routine.id == routineId)
                {
                    isPaused = routine.paused;
                }
            }

            return isPaused;
        }

        private static void StartRoutine(DRRoutine routine,
            GameObject routineGameObject,
            IEnumerator<IDRInstruction> routineInstruction)
        {
            routine.Start(routineGameObject, routineInstruction);
            if (routine.index > _routineMaxIndex)
            {
                _routineMaxIndex = routine.index;
            }
        }

        private static void StopRoutine(DRRoutine routine)
        {
            routine.Stop();
            if (routine.index == _routineMaxIndex)
            {
                int updatedRoutineMaxIndex = -1;
                for (int idx = _routineMaxIndex; idx >= 0; --idx)
                {
                    if (_routines[idx].active)
                    {
                        updatedRoutineMaxIndex = _routines[idx].index;
                        break;
                    }
                }
                _routineMaxIndex = updatedRoutineMaxIndex;
            }

            // TODO : Remove before finalizing
            if (_routineMaxIndex < -1)
            {
                Debug.LogError("RoutineSystem.StopRoutine : _routineMaxIndex is less than -1 : " + _routineMaxIndex);
            }
        }

        private void Update()
        {
            if (_routineMaxIndex > -1)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    DRRoutine routine = _routines[idx];
                    if(routine.active)
                    {
                        // INFO : Detect type
                        bool routineScrub = false;
                        IDRInstruction routineInstruction = null;
                        if (routine.type == DRType.None)
                        {
                            routineInstruction = routine.instruction.Current;
                            if (routineInstruction == null)
                            {
                                routine.type = DRType.Update;
                            }
                            else
                            {
                                routine.type = routineInstruction.type;
                            }

                            routineScrub = true;
                        }

                        if (routine.type == DRType.Update)
                        {
                            // INFO : Boilerplate
                            if (routine.gameObject == null)
                            {
                                StopRoutine(routine);
                                continue;
                            }

                            if (routine.paused)
                            {
                                continue;
                            }

                            // INFO : Type specfic
                            if (!routineScrub)
                            {
                                routineInstruction = routine.instruction.Current;
                                if (routineInstruction != null && routineInstruction.type != DRType.Update)
                                {
                                    routine.type = routineInstruction.type;
                                    continue;
                                }
                            }

                            if (!routine.instruction.MoveNext())
                            {
                                StopRoutine(routine);
                                continue;
                            }
                        }
                        else if (routine.type == DRType.TimedUpdate)
                        {
                            // INFO : Boilerplate
                            if (routine.gameObject == null)
                            {
                                StopRoutine(routine);
                                continue;
                            }

                            if (routine.paused)
                            {
                                continue;
                            }

                            // INFO : Type specfic
                            if (!routineScrub)
                            {
                                routineInstruction = routine.instruction.Current;
                                if (routineInstruction == null)
                                {
                                    routine.deltaTime = 0;
                                    routine.type = DRType.Update;
                                    continue;
                                }
                                else if (routineInstruction.type != DRType.TimedUpdate)
                                {
                                    routine.deltaTime = 0;
                                    routine.type = routineInstruction.type;
                                    continue;
                                }
                            }

                            routine.deltaTime += Time.unscaledDeltaTime;
                            if (routine.deltaTime >= routineInstruction.time)
                            {
                                routine.deltaTime = 0;
                                if (!routine.instruction.MoveNext())
                                {
                                    StopRoutine(routine);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (_routineMaxIndex > -1)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    DRRoutine routine = _routines[idx];
                    if(routine.active)
                    {
                        // INFO : Detect type
                        bool routineScrub = false;
                        IDRInstruction routineInstruction = null;
                        if (routine.type == DRType.None)
                        {
                            routineInstruction = routine.instruction.Current;
                            if (routineInstruction == null)
                            {
                                routine.type = DRType.Update;
                            }
                            else
                            {
                                routine.type = routineInstruction.type;
                            }

                            routineScrub = true;
                        }

                        if (routine.type == DRType.LateUpdate)
                        {
                            // INFO : Boilerplate
                            if (routine.gameObject == null)
                            {
                                StopRoutine(routine);
                                continue;
                            }

                            if (routine.paused)
                            {
                                continue;
                            }

                            // INFO : Type specfic
                            if (!routineScrub)
                            {
                                routineInstruction = routine.instruction.Current;
                                if (routineInstruction == null)
                                {
                                    routine.type = DRType.Update;
                                    continue;
                                }
                                else if (routineInstruction.type != DRType.LateUpdate)
                                {
                                    routine.type = routineInstruction.type;
                                    continue;
                                }
                            }

                            if (!routine.instruction.MoveNext())
                            {
                                StopRoutine(routine);
                                continue;
                            }
                        }
                    }
                }
            }
        }

#if DR_FIXEDUPDATE
        private void FixedUpdate()
        {
            if (_routineMaxIndex > -1)
            {
                for (int idx = 0; idx <= _routineMaxIndex; ++idx)
                {
                    DRRoutine routine = _routines[idx];
                    if(routine.active)
                    {
                        // INFO : Detect type
                        bool routineScrub = false;
                        IDRInstruction routineInstruction = null;
                        if (routine.type == DRType.None)
                        {
                            routineInstruction = routine.instruction.Current;
                            if (routineInstruction == null)
                            {
                                routine.type = DRType.Update;
                            }
                            else
                            {
                                routine.type = routineInstruction.type;
                            }

                            routineScrub = true;
                        }

                        if (routine.type == DRType.FixedUpdate)
                        {
                            // INFO : Boilerplate
                            if (routine.gameObject == null)
                            {
                                StopRoutine(routine);
                                continue;
                            }

                            if (routine.paused)
                            {
                                continue;
                            }

                            // INFO : Type specfic
                            if (!routineScrub)
                            {
                                routineInstruction = routine.instruction.Current;
                                if (routineInstruction == null)
                                {
                                    routine.type = DRType.Update;
                                    continue;
                                }
                                else if (routineInstruction.type != DRType.FixedUpdate)
                                {
                                    routine.type = routineInstruction.type;
                                    continue;
                                }
                            }

                            if (!routine.instruction.MoveNext())
                            {
                                StopRoutine(routine);
                                continue;
                            }
                        }
                    }
                }
            }
        }
#endif

        /*!
            ToString
        */
        public override string ToString()
        {
            int activeRoutineUpdateCount = 0;
            int pausedRoutineUpdateCount = 0;
            int activeRoutineTimedUpdateCount = 0;
            int pausedRoutineTimedUpdateCount = 0;
            int activeRoutineLateUpdateCount = 0;
            int pausedRoutineLateUpdateCount = 0;
#if DR_FIXEDUPDATE
            int activeRoutineFixedUpdateCount = 0;
            int pausedRoutineFixedUpdateCount = 0;
#endif
            for (int idx = 0; idx <= _routineMaxIndex; ++idx)
            {
                DRRoutine routine = _routines[idx];
                switch (routine.type)
                {
                    case DRType.Update:
                    ++activeRoutineUpdateCount;
                    if (routine.paused) ++pausedRoutineUpdateCount;
                    break;
                    case DRType.TimedUpdate:
                    ++activeRoutineTimedUpdateCount;
                    if (routine.paused) ++pausedRoutineTimedUpdateCount;
                    break;
                    case DRType.LateUpdate:
                    ++activeRoutineLateUpdateCount;
                    if (routine.paused) ++pausedRoutineLateUpdateCount;
                    break;
#if DR_FIXEDUPDATE
                    case DRType.FixedUpdate:
                    ++activeRoutineFixedUpdateCount;
                    if (routine.paused) ++pausedRoutineFixedUpdateCount;
                    break;
#endif
                    default:
                    break;
                }
            }

            int pausedRoutineCount = pausedRoutineUpdateCount + pausedRoutineTimedUpdateCount + pausedRoutineLateUpdateCount;
#if DR_FIXEDUPDATE
            pausedRoutineCount += pausedRoutineFixedUpdateCount;
#endif

            StringBuilder description = new StringBuilder();
            description.AppendFormat("General : {0}", System.Environment.NewLine);
            description.AppendFormat("isInit : {0}{1}", isInit, System.Environment.NewLine);
            description.AppendFormat("Max Routine Count : {0}{1}", _routineMax, System.Environment.NewLine);
            description.AppendFormat("Active Routine Count : {0}{1}", activeRoutineCount, System.Environment.NewLine);
            description.AppendFormat("Paused Routine Count : {0}{1}{2}", pausedRoutineCount, System.Environment.NewLine, System.Environment.NewLine);
            description.AppendFormat("Update : {0}", System.Environment.NewLine);
            description.AppendFormat("Active Routine Count : {0}{1}", activeRoutineUpdateCount, System.Environment.NewLine);
            description.AppendFormat("Paused Routine Count : {0}{1}{2}", pausedRoutineUpdateCount, System.Environment.NewLine, System.Environment.NewLine);
            description.AppendFormat("TimedUpdate : {0}", System.Environment.NewLine);
            description.AppendFormat("Active Routine Count : {0}{1}", activeRoutineTimedUpdateCount, System.Environment.NewLine);
            description.AppendFormat("Paused Routine Count : {0}{1}{2}", pausedRoutineTimedUpdateCount, System.Environment.NewLine, System.Environment.NewLine);
            description.AppendFormat("LateUpdate : {0}", System.Environment.NewLine);
            description.AppendFormat("Active Routine Count : {0}{1}", activeRoutineLateUpdateCount, System.Environment.NewLine);
            description.AppendFormat("Paused Routine Count : {0}{1}{2}", pausedRoutineLateUpdateCount, System.Environment.NewLine, System.Environment.NewLine);
#if DR_FIXEDUPDATE
            description.AppendFormat("FixedUpdate : {0}", System.Environment.NewLine);
            description.AppendFormat("Active Routine Count : {0}{1}", activeRoutineFixedUpdateCount, System.Environment.NewLine);
            description.AppendFormat("Paused Routine Count : {0}{1}", pausedRoutineFixedUpdateCount, System.Environment.NewLine);
#endif
            return description.ToString();
        }
    }
}
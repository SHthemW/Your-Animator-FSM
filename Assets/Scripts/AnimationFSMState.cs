using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yours.AnimationFSM
{
    public abstract class AnimationFSMState : StateMachineBehaviour
    {
        protected Animator _animator { get; private set; }

        private bool _isInited = false;
        private bool _InitFailed = false;

        public override sealed void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_isInited)
            {
                try
                {
                    Init(animator);
                }
                catch (Exception e)
                {
                    _InitFailed = true;
                    throw e;
                }
                _isInited = true;
            }

            if (_InitFailed)
                return;

            base.OnStateEnter(animator, stateInfo, layerIndex);
            EnterStateAction();
        }

        public override sealed void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_InitFailed)
                return;

            base.OnStateUpdate(animator, stateInfo, layerIndex);
            UpdateStateAction();
        }

        public override sealed void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_InitFailed)
                return;

            base.OnStateExit(animator, stateInfo, layerIndex);
            ExitStateAction();
        }

        protected virtual void Init(Animator obj)
        {
            _animator = obj;
        }
        protected virtual void EnterStateAction() { }
        protected virtual void UpdateStateAction() { }
        protected virtual void ExitStateAction() { }

        protected static TBehv GetBehaviour<TBehv>(Animator obj, bool must = true) where TBehv : IBehaviourHolder
        {
            var component = obj.GetComponentInParent<TBehv>();

            if (component == null && must)
                throw new MissingComponentException(typeof(TBehv).Name);

            return component;
        }

        protected static TProp GetStateProp<TProp>(Animator obj) where TProp : IStateProperty
        {
            var holder = GetBehaviour<IStatePropertyHolder>(obj, true);

            foreach (var field in holder.GetType().GetFields())
            {
                if (!Attribute.IsDefined(field, typeof(StatePropertyAttribute)))
                    continue;

                if (field is not IStateProperty)
                    throw new Exception($"[err] field {field.Name} with {nameof(StatePropertyAttribute)} is not a {nameof(IStateProperty)}, which is not allowd.");

                return (TProp)field.GetValue(holder);
            }

            throw new Exception($"[err] cannot found any field with {nameof(StatePropertyAttribute)} in {holder.name}.");
        }
    }
}

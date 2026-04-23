import { ref, computed } from 'vue'
import { useQuery, useMutation, useSubscription } from '@vue/apollo-composable'
import { GET_PLANS, CREATE_PLAN, ADD_STEP, ON_PLAN_CREATED, ON_PLAN_UPDATED } from '../graphql/operations'

export function usePlans() {
  const notifications = ref([])

  // ── Queries ─────────────────────────────────────────────────────────────────
  const { result, loading, refetch } = useQuery(GET_PLANS, null, {
    fetchPolicy: 'cache-and-network'
  })

  const plans = computed(() => result.value?.plans ?? [])

  // ── Mutations ───────────────────────────────────────────────────────────────
  const { mutate: createPlanMutation, loading: creating } = useMutation(CREATE_PLAN, {
    update(cache, { data: { createPlan } }) {
      const existing = cache.readQuery({ query: GET_PLANS })
      if (existing) {
        cache.writeQuery({
          query: GET_PLANS,
          data: { plans: [createPlan, ...existing.plans] }
        })
      }
    }
  })

  const { mutate: addStepMutation, loading: addingStep } = useMutation(ADD_STEP, {
    update(cache, { data: { addStep } }) {
      const existing = cache.readQuery({ query: GET_PLANS })
      if (existing) {
        const updated = existing.plans.map(p =>
          p.id === addStep.planId
            ? { ...p, steps: [...p.steps, addStep] }
            : p
        )
        cache.writeQuery({ query: GET_PLANS, data: { plans: updated } })
      }
    }
  })

  // ── Subscriptions ───────────────────────────────────────────────────────────
  const { onResult: onCreated } = useSubscription(ON_PLAN_CREATED)
  onCreated(({ data }) => {
    const e = data?.onPlanCreated
    if (!e) return
    pushNotification('success', `✦ Plan created`, `"${e.title}" is ready`)
    refetch()
  })

  // ── Actions ─────────────────────────────────────────────────────────────────
  async function createPlan(title, description) {
    return await createPlanMutation({ title, description })
  }

  async function addStep(planId, title, description, order) {
    return await addStepMutation({ planId, title, description, order })
  }

  function subscribeToPlan(planId, onUpdate) {
    return onUpdate
  }

  function pushNotification(type, title, message) {
    const id = Date.now()
    notifications.value.unshift({ id, type, title, message, ts: new Date() })
    setTimeout(() => {
      notifications.value = notifications.value.filter(n => n.id !== id)
    }, 5000)
  }

  return {
    plans, loading, creating, addingStep,
    notifications,
    createPlan, addStep, subscribeToPlan,
    pushNotification, refetch
  }
}

export function usePlanSubscription(planId, onEvent) {
  const { onResult } = useSubscription(ON_PLAN_UPDATED, { planId })
  onResult(({ data }) => {
    const e = data?.onPlanUpdated
    if (e) onEvent(e)
  })
}

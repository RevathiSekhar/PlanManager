import { gql } from '@apollo/client/core'

// ── Queries ───────────────────────────────────────────────────────────────────

export const GET_PLANS = gql`
  query GetPlans {
    plans {
      id
      title
      description
      status
      createdAt
      updatedAt
      steps {
        id
        title
        description
        order
        status
        createdAt
      }
    }
  }
`

export const GET_PLAN = gql`
  query GetPlan($id: ID!) {
    plan(id: $id) {
      id
      title
      description
      status
      createdAt
      updatedAt
      steps {
        id
        title
        description
        order
        status
        createdAt
      }
    }
  }
`

// ── Mutations ─────────────────────────────────────────────────────────────────

export const CREATE_PLAN = gql`
  mutation CreatePlan($title: String!, $description: String) {
    createPlan(input: { title: $title, description: $description }) {
      id
      title
      description
      status
      createdAt
      updatedAt
      steps { id }
    }
  }
`

export const ADD_STEP = gql`
  mutation AddStep($planId: ID!, $title: String!, $description: String, $order: Int!) {
    addStep(input: { planId: $planId, title: $title, description: $description, order: $order }) {
      id
      planId
      title
      description
      order
      status
      createdAt
    }
  }
`

// ── Subscriptions ─────────────────────────────────────────────────────────────

export const ON_PLAN_CREATED = gql`
  subscription OnPlanCreated {
    onPlanCreated {
      planId
      title
      occurredAt
    }
  }
`

export const ON_PLAN_UPDATED = gql`
  subscription OnPlanUpdated($planId: ID!) {
    onPlanUpdated(planId: $planId) {
      planId
      eventType
      message
      occurredAt
    }
  }
`

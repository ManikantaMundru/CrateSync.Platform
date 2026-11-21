<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import {
  CBadge,
  CCard,
  CCardBody,
} from '@coreui/vue'

const route = useRoute()

const title = computed(
  () =>
    String(
      route.meta.title ??
        'CrateSync',
    ),
)
</script>

<template>
  <section class="page-stack">
    <div class="page-heading">
      <div>
        <p class="eyebrow">
          Organisation module
        </p>
        <h1>{{ title }}</h1>
        <p>
          The responsive application shell is
          ready. This module will be built in
          the next feature slice.
        </p>
      </div>

      <CBadge color="secondary">
        Coming next
      </CBadge>
    </div>

    <CCard class="empty-state">
      <CCardBody>
        <h3>
          {{ title }} workspace
        </h3>
        <p>
          Routing, tenant context and
          authorization are already in place.
        </p>
      </CCardBody>
    </CCard>
  </section>
</template>

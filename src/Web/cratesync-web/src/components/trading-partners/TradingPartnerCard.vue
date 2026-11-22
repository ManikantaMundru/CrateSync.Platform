<script setup lang="ts">
import type { TradingPartnerSummary } from '@/types/tradingPartners'

defineProps<{
  partner: TradingPartnerSummary
  expanded: boolean
}>()

const emit = defineEmits<{
  (event: 'toggle'): void
}>()

function initials(name: string) {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('')
}
</script>

<template>
  <article
    class="trading-partner-card"
    :class="{ expanded }"
  >
    <button
      type="button"
      class="trading-partner-card-trigger"
      :aria-expanded="expanded"
      @click="emit('toggle')"
    >
      <span class="trading-partner-initials">
        {{ initials(partner.name) }}
      </span>

      <span class="trading-partner-card-copy">
        <strong>{{ partner.name }}</strong>
        <small>
          {{
            partner.commercialIdentifier ||
            'No commercial ID'
          }}
          <span v-if="partner.country">
            · {{ partner.country }}
          </span>
        </small>
      </span>

      <span
        v-if="partner.isActive"
        class="user-status-pill active"
      >
        <span class="status-dot" />
        Active
      </span>

      <span
        v-else
        class="user-status-pill inactive"
      >
        <span class="status-dot" />
        Inactive
      </span>

      <span
        class="product-accordion-chevron"
        :class="{ expanded }"
        aria-hidden="true"
      >
        ›
      </span>
    </button>

    <div
      v-if="expanded"
      class="trading-partner-card-detail"
    >
      <slot />
    </div>
  </article>
</template>

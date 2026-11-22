<script setup lang="ts">
import type { TradingPartnerSummary } from '@/types/tradingPartners'

defineProps<{
  partners: TradingPartnerSummary[]
  entityLabel: string
}>()

const emit = defineEmits<{
  (event: 'select', partner: TradingPartnerSummary): void
}>()

function initials(name: string) {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('')
}

function addressSummary(
  partner: TradingPartnerSummary,
) {
  return [
    partner.addressLine1,
    partner.city,
    partner.stateOrProvince,
    partner.country,
  ]
    .filter(Boolean)
    .join(', ') || 'No address configured'
}
</script>

<template>
  <div class="table-responsive">
    <table class="user-table trading-partner-table">
      <thead>
        <tr>
          <th>{{ entityLabel }}</th>
          <th>Commercial ID</th>
          <th>Location / Country</th>
          <th>Primary Contact</th>
          <th>Status</th>
        </tr>
      </thead>

      <tbody>
        <tr
          v-for="partner in partners"
          :key="partner.id"
          class="trading-partner-row"
          tabindex="0"
          @click="emit('select', partner)"
          @keydown.enter="emit('select', partner)"
        >
          <td>
            <div class="trading-partner-identity">
              <span class="trading-partner-initials">
                {{ initials(partner.name) }}
              </span>

              <span class="trading-partner-copy">
                <strong>{{ partner.name }}</strong>
                <small>
                  {{ addressSummary(partner) }}
                </small>
              </span>
            </div>
          </td>

          <td>
            <span
              v-if="partner.commercialIdentifier"
              class="trading-partner-id"
            >
              {{ partner.commercialIdentifier }}
            </span>
            <span v-else>—</span>
          </td>

          <td>
            <span class="trading-partner-stack">
              <strong>
                {{ partner.country || 'Not set' }}
              </strong>
              <small>
                {{
                  [
                    partner.city,
                    partner.stateOrProvince,
                  ].filter(Boolean).join(', ') || '—'
                }}
              </small>
            </span>
          </td>

          <td>
            <span class="trading-partner-stack">
              <strong>
                {{ partner.contactName || 'Not set' }}
              </strong>
              <small>
                {{ partner.phone || partner.email || '—' }}
              </small>
            </span>
          </td>

          <td>
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
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

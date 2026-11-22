<script setup lang="ts">
import { CButton } from '@coreui/vue'

const props = defineProps<{
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}>()

const emit = defineEmits<{
  (event: 'update:page', value: number): void
  (event: 'update:pageSize', value: number): void
}>()

function changePageSize(event: Event) {
  emit(
    'update:pageSize',
    Number(
      (event.target as HTMLSelectElement).value,
    ),
  )
}
</script>

<template>
  <div class="trading-partner-pager">
    <span>
      Showing
      {{
        totalCount === 0
          ? 0
          : (page - 1) * pageSize + 1
      }}
      –
      {{
        Math.min(
          page * pageSize,
          totalCount,
        )
      }}
      of
      {{ totalCount }}
    </span>

    <div class="trading-partner-pager-actions">
      <label>
        <span>Rows</span>
        <select
          :value="pageSize"
          class="form-select form-select-sm"
          @change="changePageSize"
        >
          <option :value="10">10</option>
          <option :value="25">25</option>
          <option :value="50">50</option>
        </select>
      </label>

      <span>
        Page {{ page }} of
        {{ Math.max(totalPages, 1) }}
      </span>

      <CButton
        color="secondary"
        variant="outline"
        size="sm"
        :disabled="page <= 1"
        @click="
          emit(
            'update:page',
            Math.max(1, page - 1),
          )
        "
      >
        Previous
      </CButton>

      <CButton
        color="secondary"
        variant="outline"
        size="sm"
        :disabled="
          totalPages === 0 ||
          page >= totalPages
        "
        @click="
          emit(
            'update:page',
            Math.min(
              totalPages,
              page + 1,
            ),
          )
        "
      >
        Next
      </CButton>
    </div>
  </div>
</template>

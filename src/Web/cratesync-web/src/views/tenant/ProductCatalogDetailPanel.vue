<script setup lang="ts">
import {
  CButton,
  CSpinner,
} from '@coreui/vue'

interface ProductVariety {
  id: string
  name: string
  isActive: boolean
}

interface ProductDetails {
  id: string
  name: string
  isActive: boolean
  varieties: ProductVariety[]
}

const props = defineProps<{
  product: ProductDetails | null
  loading: boolean
  error: string | null
  canManageCatalog: boolean
  saving: boolean
  varietyName: string
  draftVarieties: string[]
}>()

const emit = defineEmits<{
  (event: 'update:varietyName', value: string): void
  (event: 'queue-variety'): void
  (event: 'remove-draft-variety', index: number): void
  (event: 'save-varieties'): void
  (
    event: 'set-variety-active',
    variety: ProductVariety,
    active: boolean,
  ): void
  (event: 'deactivate-product'): void
}>()

function handleVarietyInput(
  event: Event,
) {
  emit(
    'update:varietyName',
    (event.target as HTMLInputElement).value,
  )
}

function handleVarietyKeydown(
  event: KeyboardEvent,
) {
  if (
    event.key !== 'Enter' &&
    event.key !== ','
  ) {
    return
  }

  event.preventDefault()
  emit('queue-variety')
}
</script>

<template>
  <div
    class="catalog-detail-panel"
    aria-live="polite"
  >
    <div
      v-if="props.error"
      class="status-message status-error modal-status-message"
    >
      {{ props.error }}
    </div>

    <div
      v-if="props.loading"
      class="loading-state catalog-detail-loading"
    >
      <CSpinner />
      Loading product…
    </div>

    <template v-else-if="props.product">
      <div class="catalog-detail-header">
        <div>
          <p class="eyebrow">
            Product
          </p>
          <h2>
            {{ props.product.name }}
          </h2>
        </div>

        <span
          v-if="props.product.isActive"
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
      </div>

      <div class="product-varieties-section">
        <div class="section-heading product-detail-heading">
          <div>
            <h3>Varieties</h3>
            <p>
              {{ props.product.varieties.length }}
              configured
            </p>
          </div>
        </div>

        <div
          v-if="
            props.canManageCatalog &&
            props.product.isActive
          "
          class="variety-editor"
        >
          <div class="variety-input-row catalog-variety-input">
            <input
              :value="props.varietyName"
              class="form-control"
              maxlength="150"
              placeholder="Type a variety"
              @input="handleVarietyInput"
              @keydown="handleVarietyKeydown"
            >

            <CButton
              color="secondary"
              variant="outline"
              type="button"
              :disabled="
                props.saving ||
                !props.varietyName.trim()
              "
              @click="emit('queue-variety')"
            >
              <i
                class="button-icon cil-plus"
                aria-hidden="true"
              />
              Add variety
            </CButton>

            <CButton
              color="primary"
              type="button"
              :disabled="
                props.saving ||
                props.draftVarieties.length === 0
              "
              @click="emit('save-varieties')"
            >
              Save varieties
            </CButton>
          </div>

          <div
            v-if="props.draftVarieties.length"
            class="variety-pill-group draft"
          >
            <span
              v-for="(name, index) in props.draftVarieties"
              :key="`${name}-${index}`"
              class="variety-pill draft"
            >
              {{ name }}
              <button
                type="button"
                aria-label="Remove draft variety"
                @click="
                  emit(
                    'remove-draft-variety',
                    index,
                  )
                "
              >
                ×
              </button>
            </span>
          </div>
        </div>

        <div
          v-if="props.product.varieties.length"
          class="variety-pill-group saved"
        >
          <span
            v-for="variety in props.product.varieties"
            :key="variety.id"
            class="variety-pill saved"
            :class="{
              inactive: !variety.isActive,
            }"
          >
            {{ variety.name }}
            <span
              class="variety-pill-status"
              :title="
                variety.isActive
                  ? 'Active'
                  : 'Inactive'
              "
            />

            <button
              v-if="
                props.canManageCatalog &&
                props.product.isActive
              "
              type="button"
              class="saved-variety-action"
              :title="
                variety.isActive
                  ? 'Deactivate variety'
                  : 'Reactivate variety'
              "
              :aria-label="
                variety.isActive
                  ? `Deactivate ${variety.name}`
                  : `Reactivate ${variety.name}`
              "
              :disabled="props.saving"
              @click="
                emit(
                  'set-variety-active',
                  variety,
                  !variety.isActive,
                )
              "
            >
              {{
                variety.isActive
                  ? '×'
                  : '↻'
              }}
            </button>
          </span>
        </div>

        <div
          v-else
          class="catalog-empty-inline"
        >
          No varieties configured yet.
        </div>
      </div>

      <div
        v-if="
          props.canManageCatalog &&
          props.product.isActive
        "
        class="catalog-detail-danger-zone"
      >
        <div>
          <strong>
            Deactivate product
          </strong>
          <p>
            Keeps historical references intact
            while preventing new operational use.
          </p>
        </div>

        <CButton
          color="danger"
          variant="outline"
          size="sm"
          :disabled="props.saving"
          @click="emit('deactivate-product')"
        >
          <i
            class="button-icon cil-ban"
            aria-hidden="true"
          />
          Deactivate
        </CButton>
      </div>
    </template>

    <div
      v-else
      class="catalog-empty-inline"
    >
      Select a product to view its details.
    </div>
  </div>
</template>

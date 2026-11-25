<script setup lang="ts">
import {
  computed,
  nextTick,
  onBeforeUnmount,
  onMounted,
  reactive,
  ref,
  watch,
} from 'vue'
import {
  CButton,
  CCard,
  CCardBody,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CSpinner,
} from '@coreui/vue'
import {
  createColumnHelper,
  createPaginatedRowModel,
  rowPaginationFeature,
  tableFeatures,
  useTable,
} from '@tanstack/vue-table'
import { apiFetch } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import ProductCatalogDetailPanel from './ProductCatalogDetailPanel.vue'

interface ProductSummary {
  id: string
  name: string
  isActive: boolean
  varietyCount: number
}

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

interface CreateProductResponse {
  id: string
}

const auth = useAuthStore()
const products = ref<ProductSummary[]>([])
const loading = ref(true)
const saving = ref(false)
const search = ref('')
const statusFilter = ref('')
const error = ref<string | null>(null)
const success = ref<string | null>(null)

const createModalVisible = ref(false)
const createModalError = ref<string | null>(null)
const createForm = reactive({
  name: '',
})

const detailsLoading = ref(false)
const detailsError = ref<string | null>(null)
const selectedProduct = ref<ProductDetails | null>(null)
const isCompactCatalog = ref(false)
const expandedProductId = ref<string | null>(null)
let compactCatalogMedia: MediaQueryList | null = null
const varietyName = ref('')
const draftVarieties = ref<string[]>([])

const canManageCatalog = computed(
  () =>
    auth.me?.currentTenant?.accessMode ===
      'SuperAdmin' ||
    auth.me?.currentTenant?.roles.includes(
      'TenantOwner',
    ) === true ||
    currentTenant?.roles.includes(
      'TenantAdmin',
    ) === true,
)

const filteredProducts = computed(() => {
  const term = search.value.trim().toLowerCase()

  return products.value.filter((product) => {
    const matchesSearch =
      !term ||
      product.name.toLowerCase().includes(term)

    const matchesStatus =
      !statusFilter.value ||
      (statusFilter.value === 'active'
        ? product.isActive
        : !product.isActive)

    return matchesSearch && matchesStatus
  })
})

const productTableFeatures = tableFeatures({
  rowPaginationFeature,
  paginatedRowModel: createPaginatedRowModel(),
})

const productColumnHelper =
  createColumnHelper<
    typeof productTableFeatures,
    ProductSummary
  >()

const productColumns =
  productColumnHelper.columns([
    productColumnHelper.accessor('name', {
      header: 'Product',
    }),
    productColumnHelper.accessor(
      'varietyCount',
      {
        header: 'Varieties',
      },
    ),
    productColumnHelper.accessor(
      'isActive',
      {
        header: 'Status',
      },
    ),
  ])

const productTable = useTable({
  key: 'catalog-products',
  features: productTableFeatures,
  data: filteredProducts,
  columns: productColumns,
  initialState: {
    pagination: {
      pageIndex: 0,
      pageSize: 10,
    },
  },
})

const pagination = computed(
  () => productTable.atoms.pagination.get(),
)

const pageCount = computed(
  () => productTable.getPageCount(),
)


const visibleProductRows = computed(
  () => productTable.getRowModel().rows,
)

const firstVisibleProduct = computed(() => {
  if (filteredProducts.value.length === 0)
    return 0

  return (
    pagination.value.pageIndex *
      pagination.value.pageSize +
    1
  )
})

const lastVisibleProduct = computed(() =>
  Math.min(
    filteredProducts.value.length,
    (pagination.value.pageIndex + 1) *
      pagination.value.pageSize,
  ),
)

watch([search, statusFilter], async () => {
  productTable.setPageIndex(0)
  await nextTick()
})

watch(
  () =>
    visibleProductRows.value
      .map((row) => row.original.id)
      .join('|'),
  async () => {
    const rows = visibleProductRows.value

    if (rows.length === 0) {
      selectedProduct.value = null
      expandedProductId.value = null
      return
    }

    if (isCompactCatalog.value) {
      const expandedIsVisible =
        expandedProductId.value &&
        rows.some(
          (row) =>
            row.original.id ===
            expandedProductId.value,
        )

      if (!expandedIsVisible) {
        expandedProductId.value = null
        selectedProduct.value = null
      }

      return
    }

    const selectedIsVisible =
      selectedProduct.value &&
      rows.some(
        (row) =>
          row.original.id ===
          selectedProduct.value?.id,
      )

    if (!selectedIsVisible) {
      await selectProduct(rows[0]!.original)
    }
  },
)

function changePageSize(event: Event) {
  const value = Number(
    (event.target as HTMLSelectElement).value,
  )

  productTable.setPageSize(value)
}

async function loadProducts() {
  loading.value = true
  error.value = null

  try {
    products.value =
      await apiFetch<ProductSummary[]>(
        '/api/catalog/products',
      )

    await nextTick()

    if (
      !isCompactCatalog.value &&
      !selectedProduct.value &&
      visibleProductRows.value.length > 0
    ) {
      await selectProduct(
        visibleProductRows.value[0]!.original,
      )
    }
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to load products.'
  } finally {
    loading.value = false
  }
}

async function syncCatalogLayout(
  matches: boolean,
) {
  isCompactCatalog.value = matches

  if (matches) {
    expandedProductId.value = null
    selectedProduct.value = null
    ++productSelectionRequest
    detailsLoading.value = false
    return
  }

  expandedProductId.value = null

  if (
    visibleProductRows.value.length > 0 &&
    !selectedProduct.value
  ) {
    await selectProduct(
      visibleProductRows.value[0]!.original,
    )
  }
}

function handleCatalogMediaChange(
  event: MediaQueryListEvent,
) {
  void syncCatalogLayout(event.matches)
}

onMounted(async () => {
  compactCatalogMedia = window.matchMedia(
    '(max-width: 991.98px)',
  )

  isCompactCatalog.value =
    compactCatalogMedia.matches

  compactCatalogMedia.addEventListener(
    'change',
    handleCatalogMediaChange,
  )

  await loadProducts()
})

onBeforeUnmount(() => {
  compactCatalogMedia?.removeEventListener(
    'change',
    handleCatalogMediaChange,
  )
})

function openCreateModal() {
  createModalError.value = null
  createForm.name = ''
  createModalVisible.value = true
}

function closeCreateModal() {
  if (!saving.value) {
    createModalError.value = null
    createModalVisible.value = false
  }
}

async function createProduct() {
  saving.value = true
  createModalError.value = null
  success.value = null

  try {
    const response =
      await apiFetch<CreateProductResponse>(
        '/api/catalog/products',
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({
            name: createForm.name.trim(),
          }),
        },
      )

    createModalVisible.value = false
    success.value =
      'Product created successfully.'

    await loadProducts()

    if (!isCompactCatalog.value) {
      await selectProduct({
        id: response.id,
        name: createForm.name.trim(),
        isActive: true,
        varietyCount: 0,
      })
    }
  } catch (exception) {
    createModalError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to create product.'
  } finally {
    saving.value = false
  }
}

let productSelectionRequest = 0

async function selectProduct(
  product: ProductSummary,
) {
  const requestId =
    ++productSelectionRequest

  detailsLoading.value = true
  detailsError.value = null
  varietyName.value = ''
  draftVarieties.value = []

  try {
    const details =
      await apiFetch<ProductDetails>(
        `/api/catalog/products/${product.id}`,
      )

    if (
      requestId === productSelectionRequest
    ) {
      selectedProduct.value = details
    }
  } catch (exception) {
    if (
      requestId === productSelectionRequest
    ) {
      detailsError.value =
        exception instanceof Error
          ? exception.message
          : 'Unable to load product details.'
    }
  } finally {
    if (
      requestId === productSelectionRequest
    ) {
      detailsLoading.value = false
    }
  }
}

async function toggleProduct(
  product: ProductSummary,
) {
  if (!isCompactCatalog.value) {
    await selectProduct(product)
    return
  }

  if (expandedProductId.value === product.id) {
    expandedProductId.value = null
    selectedProduct.value = null
    detailsError.value = null
    detailsLoading.value = false
    ++productSelectionRequest
    return
  }

  expandedProductId.value = product.id
  await selectProduct(product)
}

function isProductExpanded(
  productId: string,
) {
  return isCompactCatalog.value
    ? expandedProductId.value === productId
    : selectedProduct.value?.id === productId
}

async function refreshSelectedProduct() {
  if (!selectedProduct.value)
    return

  const id = selectedProduct.value.id

  selectedProduct.value =
    await apiFetch<ProductDetails>(
      `/api/catalog/products/${id}`,
    )
}

function queueVarietyInput() {
  if (!selectedProduct.value)
    return

  const candidates =
    varietyName.value
      .split(',')
      .map((name) => name.trim())
      .filter(Boolean)

  if (candidates.length === 0)
    return

  const existingNames =
    new Set(
      selectedProduct.value.varieties
        .map((variety) =>
          variety.name.toLowerCase(),
        ),
    )

  const draftNames =
    new Set(
      draftVarieties.value.map((name) =>
        name.toLowerCase(),
      ),
    )

  for (const candidate of candidates) {
    if (candidate.length > 150) {
      detailsError.value =
        'Variety names cannot exceed 150 characters.'
      continue
    }

    const normalized = candidate.toLowerCase()

    if (
      existingNames.has(normalized) ||
      draftNames.has(normalized)
    ) {
      continue
    }

    draftVarieties.value.push(candidate)
    draftNames.add(normalized)
  }

  varietyName.value = ''
}

function removeDraftVariety(index: number) {
  draftVarieties.value.splice(index, 1)
}

async function saveVarieties() {
  if (!selectedProduct.value)
    return

  if (draftVarieties.value.length === 0)
    return

  saving.value = true
  detailsError.value = null
  success.value = null

  const pending = [...draftVarieties.value]
  let savedCount = 0

  try {
    for (const name of pending) {
      await apiFetch(
        `/api/catalog/products/${selectedProduct.value.id}/varieties`,
        {
          method: 'POST',
          headers: {
            'Content-Type':
              'application/json',
          },
          body: JSON.stringify({ name }),
        },
      )

      savedCount += 1
    }

    draftVarieties.value = []
    success.value =
      savedCount === 1
        ? 'Variety added successfully.'
        : `${savedCount} varieties added successfully.`
  } catch (exception) {
    draftVarieties.value =
      pending.slice(savedCount)

    detailsError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to save all varieties.'
  } finally {
    try {
      await Promise.all([
        refreshSelectedProduct(),
        loadProducts(),
      ])
    } finally {
      saving.value = false
    }
  }
}

async function setVarietyActive(
  variety: ProductVariety,
  active: boolean,
) {
  if (!selectedProduct.value)
    return

  if (
    !active &&
    !window.confirm(
      `Deactivate ${variety.name}? Historical sales and inventory references will be preserved.`,
    )
  ) {
    return
  }

  saving.value = true
  detailsError.value = null
  success.value = null

  try {
    await apiFetch(
      `/api/catalog/products/${selectedProduct.value.id}/varieties/${variety.id}/${active ? 'activate' : 'deactivate'}`,
      {
        method: 'PATCH',
      },
    )

    success.value =
      active
        ? `${variety.name} reactivated.`
        : `${variety.name} deactivated.`

    await Promise.all([
      refreshSelectedProduct(),
      loadProducts(),
    ])
  } catch (exception) {
    detailsError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to update variety status.'
  } finally {
    saving.value = false
  }
}

async function deactivateProduct() {
  if (!selectedProduct.value)
    return

  if (
    !window.confirm(
      `Deactivate ${selectedProduct.value.name}? It will no longer be available for new operational use.`,
    )
  ) {
    return
  }

  saving.value = true
  detailsError.value = null
  success.value = null

  try {
    await apiFetch(
      `/api/catalog/products/${selectedProduct.value.id}/deactivate`,
      {
        method: 'PATCH',
      },
    )

    success.value =
      'Product deactivated.'
    await Promise.all([
      refreshSelectedProduct(),
      loadProducts(),
    ])
  } catch (exception) {
    detailsError.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to deactivate product.'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <section class="page-stack users-page">
    <div class="page-heading catalog-page-heading">
      <div>
        <p class="eyebrow catalog-page-eyebrow">
          Catalog
        </p>
        <h1>Products</h1>
        <p>
          Manage products and varieties used
          across trading, inbound logistics and
          inventory.
        </p>
      </div>

      <CButton
        v-if="
          isCompactCatalog &&
          canManageCatalog
        "
        color="primary"
        class="catalog-compact-add-button"
        @click="openCreateModal"
      >
        <i
          class="button-icon cil-plus"
          aria-hidden="true"
        />
        Add product
      </CButton>
    </div>

    <div
      v-if="error"
      class="status-message status-error"
    >
      {{ error }}
    </div>

    <div
      v-if="success"
      class="status-message status-success"
    >
      {{ success }}
    </div>

    <div class="tenant-toolbar users-toolbar catalog-toolbar">
      <div class="catalog-toolbar-copy">
        <h2>Product catalog</h2>
        <p>
          {{ filteredProducts.length }}
          of
          {{ products.length }}
          products
        </p>
      </div>

      <div class="users-toolbar-actions catalog-toolbar-actions">
        <div class="search-field catalog-search-field">
          <i
            class="cil-search"
            aria-hidden="true"
          />
          <input
            v-model="search"
            class="form-control tenant-search"
            type="search"
            placeholder="Search products..."
            aria-label="Search products"
          >
        </div>

        <div
          v-if="isCompactCatalog"
          class="catalog-compact-filter-row"
        >
          <div
            class="catalog-status-segmented"
            role="group"
            aria-label="Filter product status"
          >
            <button
              type="button"
              :class="{ active: statusFilter === '' }"
              @click="statusFilter = ''"
            >
              All
            </button>
            <button
              type="button"
              :class="{ active: statusFilter === 'active' }"
              @click="statusFilter = 'active'"
            >
              Active
            </button>
            <button
              type="button"
              :class="{ active: statusFilter === 'inactive' }"
              @click="statusFilter = 'inactive'"
            >
              Inactive
            </button>
          </div>

          <span class="catalog-compact-count">
            {{ filteredProducts.length }}
            of
            {{ products.length }}
            products
          </span>
        </div>

        <select
          v-else
          v-model="statusFilter"
          class="form-select user-role-filter"
          aria-label="Filter product status"
        >
          <option value="">
            All statuses
          </option>
          <option value="active">
            Active
          </option>
          <option value="inactive">
            Inactive
          </option>
        </select>

        <CButton
          v-if="
            !isCompactCatalog &&
            canManageCatalog
          "
          color="primary"
          class="users-add-button"
          @click="openCreateModal"
        >
          <i
            class="button-icon cil-plus"
            aria-hidden="true"
          />
          Add product
        </CButton>
      </div>
    </div>

    <div
      v-if="loading"
      class="loading-state"
    >
      <CSpinner />
      Loading products…
    </div>

    <CCard
      v-else-if="
        filteredProducts.length === 0
      "
      class="empty-state"
    >
      <CCardBody>
        <h3>
          {{
            search || statusFilter
              ? 'No matching products'
              : 'No products yet'
          }}
        </h3>
        <p>
          {{
            search || statusFilter
              ? 'Try another product name or status.'
              : canManageCatalog
                ? 'Add the first product to this organisation catalog.'
                : 'No products are available.'
          }}
        </p>
      </CCardBody>
    </CCard>

    <div
      v-else
      class="catalog-master-detail"
    >
      <section class="catalog-list-panel">
        <div class="user-table-shell catalog-list-shell">
          <div
            v-if="isCompactCatalog"
            class="product-card-list"
          >
            <article
              v-for="row in visibleProductRows"
              :key="row.id"
              class="product-accordion-card"
              :class="{
                expanded:
                  isProductExpanded(
                    row.original.id,
                  ),
              }"
            >
              <button
                type="button"
                class="product-card-trigger"
                :aria-expanded="
                  isProductExpanded(
                    row.original.id,
                  )
                "
                @click="
                  toggleProduct(row.original)
                "
              >
                <span class="product-icon">
                  <i
                    class="cil-leaf"
                    aria-hidden="true"
                  />
                </span>

                <span class="product-card-copy">
                  <strong>
                    {{ row.original.name }}
                  </strong>
                  <small>
                    {{ row.original.varietyCount }}
                    {{
                      row.original.varietyCount === 1
                        ? 'variety'
                        : 'varieties'
                    }}
                  </small>
                </span>

                <span
                  v-if="row.original.isActive"
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
                  :class="{
                    expanded:
                      isProductExpanded(
                        row.original.id,
                      ),
                  }"
                  aria-hidden="true"
                >
                  ›
                </span>
              </button>

              <ProductCatalogDetailPanel
                v-if="
                  isProductExpanded(
                    row.original.id,
                  )
                "
                compact
                :product="selectedProduct"
                :loading="detailsLoading"
                :error="detailsError"
                :can-manage-catalog="canManageCatalog"
                :saving="saving"
                :variety-name="varietyName"
                :draft-varieties="draftVarieties"
                @update:variety-name="
                  varietyName = $event
                "
                @queue-variety="queueVarietyInput"
                @remove-draft-variety="
                  removeDraftVariety
                "
                @save-varieties="saveVarieties"
                @set-variety-active="
                  setVarietyActive
                "
                @deactivate-product="
                  deactivateProduct
                "
              />
            </article>
          </div>

          <div
            v-else
            class="table-responsive"
          >
            <table class="user-table product-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Varieties</th>
                  <th>Status</th>
                </tr>
              </thead>

              <tbody>
                <tr
                  v-for="row in visibleProductRows"
                  :key="row.id"
                  class="product-select-row"
                  :class="{
                    selected:
                      isProductExpanded(
                        row.original.id,
                      ),
                  }"
                  tabindex="0"
                  @click="
                    toggleProduct(row.original)
                  "
                  @keydown.enter="
                    toggleProduct(row.original)
                  "
                >
                  <td data-label="Product">
                    <div class="product-identity">
                      <div class="product-icon">
                        <i
                          class="cil-leaf"
                          aria-hidden="true"
                        />
                      </div>

                      <strong>
                        {{ row.original.name }}
                      </strong>
                    </div>
                  </td>

                  <td data-label="Varieties">
                    <span class="product-variety-count">
                      {{ row.original.varietyCount }}
                      {{
                        row.original.varietyCount === 1
                          ? 'variety'
                          : 'varieties'
                      }}
                    </span>
                  </td>

                  <td data-label="Status">
                    <span
                      v-if="row.original.isActive"
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

          <div class="user-grid-pagination catalog-pagination">
            <div class="pagination-summary">
              Showing
              <strong>
                {{ firstVisibleProduct }}
              </strong>
              –
              <strong>
                {{ lastVisibleProduct }}
              </strong>
              of
              <strong>
                {{ filteredProducts.length }}
              </strong>
            </div>

            <div class="pagination-controls">
              <label
                v-if="!isCompactCatalog"
                class="page-size-control"
              >
                <span>Rows</span>
                <select
                  :value="pagination.pageSize"
                  class="form-select form-select-sm"
                  aria-label="Rows per page"
                  @change="changePageSize"
                >
                  <option :value="10">10</option>
                  <option :value="25">25</option>
                  <option :value="50">50</option>
                </select>
              </label>

              <span class="page-number">
                Page
                {{ pagination.pageIndex + 1 }}
                of
                {{ Math.max(pageCount, 1) }}
              </span>

              <template
                v-if="
                  !isCompactCatalog ||
                  pageCount > 1
                "
              >
                <CButton
                  color="secondary"
                  variant="outline"
                  size="sm"
                  :disabled="
                    !productTable.getCanPreviousPage()
                  "
                  @click="
                    productTable.previousPage()
                  "
                >
                  Previous
                </CButton>

                <CButton
                  color="secondary"
                  variant="outline"
                  size="sm"
                  :disabled="
                    !productTable.getCanNextPage()
                  "
                  @click="
                    productTable.nextPage()
                  "
                >
                  Next
                </CButton>
              </template>
            </div>
          </div>
        </div>
      </section>

      <ProductCatalogDetailPanel
        v-if="!isCompactCatalog"
        :product="selectedProduct"
        :loading="detailsLoading"
        :error="detailsError"
        :can-manage-catalog="canManageCatalog"
        :saving="saving"
        :variety-name="varietyName"
        :draft-varieties="draftVarieties"
        @update:variety-name="
          varietyName = $event
        "
        @queue-variety="queueVarietyInput"
        @remove-draft-variety="
          removeDraftVariety
        "
        @save-varieties="saveVarieties"
        @set-variety-active="
          setVarietyActive
        "
        @deactivate-product="
          deactivateProduct
        "
      />
    </div>

    <CModal
      :visible="createModalVisible"
      alignment="center"
      @close="closeCreateModal"
    >
      <CModalHeader>
        <CModalTitle>
          Add product
        </CModalTitle>
      </CModalHeader>

      <CModalBody>
        <div
          v-if="createModalError"
          class="status-message status-error modal-status-message"
        >
          {{ createModalError }}
        </div>

        <form
          id="create-product-form"
          @submit.prevent="createProduct"
        >
          <label class="form-field">
            <span>Product name</span>
            <input
              v-model.trim="createForm.name"
              class="form-control"
              required
              maxlength="150"
              placeholder="Apple"
            >
          </label>
        </form>
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          variant="outline"
          :disabled="saving"
          @click="closeCreateModal"
        >
          Cancel
        </CButton>

        <CButton
          color="primary"
          type="submit"
          form="create-product-form"
          :disabled="saving"
        >
          {{
            saving
              ? 'Creating…'
              : 'Create product'
          }}
        </CButton>
      </CModalFooter>
    </CModal>


  </section>
</template>

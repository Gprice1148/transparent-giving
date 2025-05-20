<template>
  <div>
    <h1>Thank you, {{ donorName }}!</h1>
    <p>
      Your donation of ${{ totalDonation.toFixed(2) }} helped the following
      children:
    </p>

    <div
      v-for="child in matchedChildren"
      :key="child.id"
      style="margin-bottom: 1rem; padding: 1rem; border: 1px solid #ccc"
    >
      <h3>{{ child.name }} ({{ child.age }}), {{ child.country }}</h3>
      <p>Surgery: {{ child.surgeryType }}</p>
      <p>
        Your contribution: ${{ child.allocated.toFixed(2) }}<br />
        New total: ${{ child.costFunded }} / ${{ child.costTotal }}
      </p>
    </div>
    <p v-if="matchedChildren.length === 0">
      Hmm — your donation didn’t get assigned. This shouldn’t happen.
    </p>

    <p v-if="surplus > 0">
      ⚠️ Note: ${{ surplus.toFixed(2) }} is still unallocated. There are no more
      children currently needing funds. This will be used later.
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";

const matchedChildren = ref<any[]>([]);
const donorName = ref("");
const totalDonation = ref(0);
const totalAllocated = ref(0);

const surplus = computed(() => {
  return totalDonation.value - totalAllocated.value;
});

onMounted(() => {
  const stored = localStorage.getItem("matchedChildren");
  if (stored) {
    matchedChildren.value = JSON.parse(stored);
  }

  const name = localStorage.getItem("donorName");
  if (name) {
    donorName.value = name;
  }

  const donation = localStorage.getItem("donationAmount");
  if (donation) {
    totalDonation.value = parseFloat(donation);
  }

  totalAllocated.value = matchedChildren.value.reduce(
    (sum, c) => sum + (c.allocated || 0),
    0
  );
});
</script>

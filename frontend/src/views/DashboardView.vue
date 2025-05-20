<template>
  <div>
    <h1>Donation Dashboard</h1>
    <p><strong>Total Donated:</strong> ${{ totalDonated }}</p>
    <p><strong>Donations Made:</strong> {{ totalCount }}</p>

    <div v-for="group in groupedDonations" :key="group.date">
      <h2>{{ formatDate(group.date) }}</h2>

      <div v-for="donation in group.donations" :key="donation.timestamp"
        style="margin-bottom: 1rem; padding: 1rem; border: 1px solid #ccc">
        <p><strong>Date:</strong> {{ formatDateTime(donation.timestamp) }}</p>
        <p><strong>Amount:</strong> ${{ donation.amount }}</p>
        <p><strong>Matches:</strong></p>
        <div v-for="match in donation.matches" :key="match.id" style="
            margin-top: 0.5rem;
            padding: 0.75rem;
            border: 1px solid #aaa;
            border-radius: 4px;
          ">
          <strong>{{ match.name }}</strong> — helped with ${{ match.allocated }}
          <br />
          <small>Country: {{ match.country }} | Surgery:
            {{ match.surgeryType }}</small>
        </div>
      </div>
    </div>
    <button @click="logout" class="text-sm text-red-600 hover:underline mt-4">
      Log out
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import api from '@/api/axios'

const donorName = localStorage.getItem("donorName") || "";
const donations = ref<any[]>([]);
const groupedDonations = ref<any[]>([]);

onMounted(async () => {
  if (!localStorage.getItem('token')) {
    router.push('/login')
    return
  }

  try {
    const userResponse = await api.get('/users/me')
    donorName.value = userResponse.data.name

    const donationsResponse = await api.get(
      `/donations/grouped?donorName=${donorName.value}`
    )
    groupedDonations.value = donationsResponse.data
  } catch (err) {
    console.error(err)
    error.value = 'Failed to load your donations'
    router.push('/login')
  } finally {
    loading.value = false
  }
})

const totalDonated = computed(() =>
  groupedDonations.value
    .flatMap((g) => g.donations)
    .reduce((sum, d) => sum + d.amount, 0)
    .toFixed(2)
);

const totalCount = computed(
  () => groupedDonations.value.flatMap((g) => g.donations).length
);

function formatDate(dateStr: string) {
  const d = new Date(dateStr + "T00:00:00"); // force local interpretation
  return d.toLocaleDateString(undefined, {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

function formatDateTime(timestamp: string) {
  return new Date(timestamp).toLocaleString();
}

const logout = () => {
  localStorage.removeItem('token')
  router.push('/login')
}
</script>

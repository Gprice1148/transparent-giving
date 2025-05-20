<template>
  <div>
    <h1>Donate</h1>

    <input v-model="donorName" type="text" placeholder="Your Name" />
    <br />

    <input v-model.number="donationAmount" type="number" placeholder="Amount" />
    <br />

    <button @click="submitDonation">Donate</button>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import axios from "axios";
import { useRouter } from "vue-router";

const donorName = ref("");
const donationAmount = ref(50);
const router = useRouter();

const submitDonation = async () => {
  if (!donorName.value || donationAmount.value <= 0) {
    alert("Please enter a name and a valid donation amount.");
    return;
  }

  try {
    const response = await axios.post("http://localhost:5181/api/donations", {
      donationAmount: donationAmount.value,
      donorName: donorName.value,
    });

    localStorage.setItem(
      "matchedChildren",
      JSON.stringify(response.data.matches)
    );
    localStorage.setItem("donorName", donorName.value);
    const surplusAmount = response.data.surplus ?? 0;
    localStorage.setItem("surplus", surplusAmount.toString());
    localStorage.setItem("donationAmount", donationAmount.value.toString());

    router.push("/success");
  } catch (error) {
    console.error("Donation failed:", error);
    alert("Something went wrong. Please try again.");
  }
};
</script>

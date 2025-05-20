<template>
    <div class="max-w-md mx-auto mt-20 p-6 border rounded-lg shadow">
        <h1 class="text-2xl font-bold mb-4">Register</h1>

        <form @submit.prevent="handleRegister" class="space-y-4">
            <input v-model="name" type="text" placeholder="Name" class="w-full border rounded p-2" required />
            <input v-model="email" type="email" placeholder="Email" class="w-full border rounded p-2" required />
            <input v-model="password" type="password" placeholder="Password" class="w-full border rounded p-2"
                required />

            <button type="submit" class="w-full bg-green-600 text-white p-2 rounded hover:bg-green-700">
                Register
            </button>

            <p v-if="error" class="text-red-600 text-sm mt-2">{{ error }}</p>
        </form>
    </div>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/api/axios'

const name = ref('')
const email = ref('')
const password = ref('')
const error = ref('')
const router = useRouter()

const handleRegister = async () => {
    error.value = ''
    try {
        await api.post('/users/register', {
            name: name.value,
            email: email.value,
            password: password.value
        })

        // Optional: auto-login after registration
        const response = await api.post('/users/login', {
            email: email.value,
            password: password.value
        })

        localStorage.setItem('token', response.data.token)
        router.push('/dashboard')
    } catch (err) {
        error.value = 'Registration failed'
    }
}
</script>
<template>
    <div class="max-w-md mx-auto mt-20 p-6 border rounded-lg shadow">
        <h1 class="text-2xl font-bold mb-4">Log In</h1>

        <form @submit.prevent="handleLogin" class="space-y-4">
            <input v-model="email" type="email" placeholder="Email" class="w-full border rounded p-2" required />
            <input v-model="password" type="password" placeholder="Password" class="w-full border rounded p-2"
                required />

            <button type="submit" class="w-full bg-blue-600 text-white p-2 rounded hover:bg-blue-700">
                Log In
            </button>

            <p v-if="error" class="text-red-600 text-sm mt-2">{{ error }}</p>
        </form>
        <router-link to="/register" class="text-sm text-blue-600 hover:underline">
            Don't have an account? Register
        </router-link>
    </div>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/api/axios'

const email = ref('')
const password = ref('')
const error = ref('')
const router = useRouter()

const handleLogin = async () => {
    error.value = ''
    try {
        const response = await api.post('/users/login', {
            email: email.value,
            password: password.value
        })

        localStorage.setItem('token', response.data.token)
        router.push('/dashboard') // redirect after login
    } catch (err) {
        error.value = 'Invalid email or password'
    }
}
</script>
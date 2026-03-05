import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics
const lockSuccessRate = new Rate('lock_success_rate');
const lockDuration = new Trend('lock_duration');
const idempotencyHits = new Counter('idempotency_cache_hits');

// Test configuration
export const options = {
  scenarios: {
    // Scenario 1: Constant load
    constant_load: {
      executor: 'constant-vus',
      vus: 10,
      duration: '2m',
    },
    
    // Scenario 2: Ramp up
    ramp_up: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 50 },
        { duration: '1m', target: 50 },
        { duration: '30s', target: 0 },
      ],
      startTime: '2m',
    },
    
    // Scenario 3: Spike test
    spike_test: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '10s', target: 100 },
        { duration: '30s', target: 100 },
        { duration: '10s', target: 0 },
      ],
      startTime: '4m',
    },
  },
  
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    http_req_failed: ['rate<0.01'],   // Error rate should be below 1%
    lock_success_rate: ['rate>0.99'], // Lock success rate should be above 99%
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  // Test 1: Lock acquisition
  const lockStart = Date.now();
  const lockRes = http.post(`${BASE_URL}/api/coordination/lock`, JSON.stringify({
    key: `resource:${__VU}:${__ITER}`,
    ttl: 5,
    action: 'process'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });
  
  const lockSuccess = check(lockRes, {
    'lock status is 200': (r) => r.status === 200,
    'lock acquired': (r) => r.json('acquired') === true,
  });
  
  lockSuccessRate.add(lockSuccess);
  lockDuration.add(Date.now() - lockStart);
  
  sleep(0.1);
  
  // Test 2: Idempotency
  const idempotencyKey = `operation:${Math.floor(__ITER / 3)}`; // Repeat every 3 iterations
  const idempotencyRes = http.post(`${BASE_URL}/api/coordination/idempotency`, JSON.stringify({
    key: idempotencyKey,
    ttl: 60,
    action: 'execute'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });
  
  check(idempotencyRes, {
    'idempotency status is 200': (r) => r.status === 200,
  });
  
  if (idempotencyRes.json('cached') === true) {
    idempotencyHits.add(1);
  }
  
  sleep(0.1);
  
  // Test 3: Saga execution
  if (__ITER % 5 === 0) { // Every 5th iteration
    const sagaRes = http.post(`${BASE_URL}/api/coordination/saga`, JSON.stringify({
      sagaId: `saga:${__VU}:${__ITER}`,
      steps: [
        { name: 'step1', action: 'validate' },
        { name: 'step2', action: 'process' },
        { name: 'step3', action: 'complete' },
      ]
    }), {
      headers: { 'Content-Type': 'application/json' },
    });
    
    check(sagaRes, {
      'saga status is 200': (r) => r.status === 200,
      'saga completed': (r) => r.json('completed') === true,
    });
  }
  
  sleep(0.5);
}

export function handleSummary(data) {
  return {
    'loadtest-summary.json': JSON.stringify(data),
    stdout: textSummary(data, { indent: ' ', enableColors: true }),
  };
}

function textSummary(data, opts) {
  const indent = opts.indent || '';
  const colors = opts.enableColors !== false;
  
  let output = `\n${indent}Load Test Summary:\n`;
  output += `${indent}================\n\n`;
  
  // HTTP metrics
  output += `${indent}HTTP Metrics:\n`;
  output += `${indent}  Requests: ${data.metrics.http_reqs.values.count}\n`;
  output += `${indent}  Duration (avg): ${data.metrics.http_req_duration.values.avg.toFixed(2)}ms\n`;
  output += `${indent}  Duration (p95): ${data.metrics.http_req_duration.values['p(95)'].toFixed(2)}ms\n`;
  output += `${indent}  Duration (p99): ${data.metrics.http_req_duration.values['p(99)'].toFixed(2)}ms\n`;
  output += `${indent}  Failed: ${(data.metrics.http_req_failed.values.rate * 100).toFixed(2)}%\n\n`;
  
  // Custom metrics
  if (data.metrics.lock_success_rate) {
    output += `${indent}Lock Metrics:\n`;
    output += `${indent}  Success Rate: ${(data.metrics.lock_success_rate.values.rate * 100).toFixed(2)}%\n`;
    output += `${indent}  Avg Duration: ${data.metrics.lock_duration.values.avg.toFixed(2)}ms\n\n`;
  }
  
  if (data.metrics.idempotency_cache_hits) {
    output += `${indent}Idempotency Metrics:\n`;
    output += `${indent}  Cache Hits: ${data.metrics.idempotency_cache_hits.values.count}\n\n`;
  }
  
  // Virtual Users
  output += `${indent}Virtual Users:\n`;
  output += `${indent}  Peak: ${data.metrics.vus_max.values.max}\n`;
  output += `${indent}  Average: ${data.metrics.vus.values.value}\n\n`;
  
  // Thresholds
  output += `${indent}Threshold Results:\n`;
  const thresholds = data.thresholds || {};
  Object.keys(thresholds).forEach(name => {
    const passed = thresholds[name].ok ? '✓' : '✗';
    output += `${indent}  ${passed} ${name}\n`;
  });
  
  return output;
}

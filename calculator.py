import json
import os
import math

def evaluate_expression(expression):
    output_queue = []
    operator_stack = []
    tokens = expression.split()

    for token in tokens:
        if is_number(token):
            output_queue.append(token)
        elif is_operator(token):
            while (operator_stack and precedence(operator_stack[-1]) >= precedence(token)):
                output_queue.append(operator_stack.pop())
            operator_stack.append(token)
        elif token == "(":
            operator_stack.append(token)
        elif token == ")":
            while operator_stack and operator_stack[-1] != "(":
                output_queue.append(operator_stack.pop())
            operator_stack.pop()
        else:
            raise ValueError(f"Invalid token: {token}")

    while operator_stack:
        output_queue.append(operator_stack.pop())

    evaluation_stack = []
    for token in output_queue:
        if is_number(token):
            evaluation_stack.append(float(token))
        elif is_operator(token):
            if token == "sqrt":
                operand = evaluation_stack.pop()
                evaluation_stack.append(math.sqrt(operand))
            else:
                right = evaluation_stack.pop()
                left = evaluation_stack.pop()
                evaluation_stack.append(apply_operator(token, left, right))

    result = evaluation_stack.pop()

    if math.isinf(result):
        raise ValueError("Result is infinity")
    if math.isnan(result):
        raise ValueError("Result is not a number (NaN)")

    return result

def is_number(token):
    try:
        float(token)
        return True
    except ValueError:
        return False

def is_operator(token):
    return token in {"+", "-", "*", "/", "sqrt"}

def precedence(op):
    return 1 if op in {"+", "-"} else 2 if op in {"*", "/", "sqrt"} else 3

def apply_operator(op, left, right):
    if op == "+":
        return left + right
    elif op == "-":
        return left - right
    elif op == "*":
        return left * right
    elif op == "/":
        return left / right
    else:
        raise ValueError(f"Invalid operator: {op}")

def save(expression, result):
    file_name = "historyPy.json"

    # Check if JSON file exists and load previous data if it does
    history = []
    if os.path.exists(file_name):
        with open(file_name, 'r') as file:
            existing_data = file.read()
            if existing_data:
                history = json.loads(existing_data)

    # Add new result to history
    history.append({"Expression": expression, "Result": str(result)})

    # Write updated history to JSON file
    with open(file_name, 'w') as file:
        json.dump(history, file, indent=4)
    print("Result saved to history.")

def show_history():
    file_name = "historyPy.json"
    
    # Check if JSON file exists and load previous data if it does
    if os.path.exists(file_name):
        with open(file_name, 'r') as file:
            existing_data = file.read()
            if existing_data:
                history = json.loads(existing_data)
                print("Calculation History:")
                for entry in history:
                    print(f"Expression: {entry['Expression']}, Result: {entry['Result']}")
            else:
                print("No history found.")
    else:
        print("No history found.")

print("Welcome to Terminal Calculator with Parser!")
continue_calculating = True

while continue_calculating:
    expression = input("Enter an expression: ")
    try:
        result = evaluate_expression(expression)
        print(f"Result: {result}")
        save(expression, result)
    except Exception as ex:
        print(f"Error: {ex}")

    continue_calculating = input("Would you like to perform another calculation? (yes/no): ").lower() == "yes"

if not continue_calculating:
    show_history_prompt = input("Would you like to see the calculation history? (yes/no): ").lower() == "yes"
    if show_history_prompt:
        show_history()

print("Thank you for using Terminal Calculator with Parser. Goodbye!")